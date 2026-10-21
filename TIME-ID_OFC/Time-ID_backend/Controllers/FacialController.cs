using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TimeId.Operations;

[ApiController, Route("api/facial")]
public sealed class FacialController(DataStore store, IDataProtectionProvider protection) : ControllerBase
{
    private static readonly string[] Kinds = { "Entrada", "Início do intervalo", "Retorno do intervalo", "Saída" };
    private readonly IDataProtector protector = protection.CreateProtector("TIMEID.FaceDescriptor.v1");
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static void Require(bool ok, string message) { if (!ok) throw new DomainError(400, message); }
    private static int Age(DateOnly birth, DateOnly today) => today.Year - birth.Year - (today.Month < birth.Month || today.Month == birth.Month && today.Day < birth.Day ? 1 : 0);
    private static bool ValidDescriptor(float[] descriptor) => descriptor.Length == 128 && descriptor.All(float.IsFinite);
    private static Account FacialViewer(Database db, Guid id)
    {
        var actor = DataStore.Actor(db, id);
        if (actor.Role is not ("Administrador" or "Gestor")) throw new DomainError(403, "Esta área é exclusiva de administradores e gestores.");
        return actor;
    }

    [Authorize, HttpGet("profiles")]
    public IActionResult Profiles() => Ok(store.Read(db =>
    {
        FacialViewer(db, ActorId);
        var today = DataStore.Today;
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        return new
        {
            employees = db.Employees.Where(e => e.Active && e.WorkMode != "Remoto").Select(e => new
            {
                e.Id, e.Name, e.Registration, e.BirthDate, Age = Age(e.BirthDate, today),
                e.DepartmentId, enrolled = db.FaceProfiles.Any(f => f.EmployeeId == e.Id),
                enrolledAt = db.FaceProfiles.FirstOrDefault(f => f.EmployeeId == e.Id)?.EnrolledAt
            }).OrderBy(e => e.Name).ToList(),
            weeklyPunches = db.Punches.Where(p => p.Source == "Facial local" && DataStore.Date(p.At) >= monday && DataStore.Date(p.At) <= today)
                .Select(p => new { p.Id, p.EmployeeId, p.Kind, p.At }).OrderByDescending(p => p.At).ToList()
        };
    }));

    [Authorize, HttpPost("profiles")]
    public IActionResult Enroll(FaceEnrollmentInput input) => Ok(store.Write(db =>
    {
        var admin = DataStore.Actor(db, ActorId); DataStore.Admin(admin);
        Require(input.IdentityVerified, "Confirme a conferência de nome, idade e matrícula com o cadastro da empresa.");
        Require(ValidDescriptor(input.Descriptor), "Não foi possível cadastrar o padrão facial. Faça a captura novamente.");
        var employee = db.Employees.FirstOrDefault(e => e.Id == input.EmployeeId && e.Active)
            ?? throw new DomainError(404, "Colaborador ativo não encontrado.");
        var profile = db.FaceProfiles.FirstOrDefault(f => f.EmployeeId == employee.Id);
        if (profile is null) { profile = new FaceProfile { EmployeeId = employee.Id }; db.FaceProfiles.Add(profile); }
        profile.ProtectedDescriptor = protector.Protect(System.Text.Json.JsonSerializer.Serialize(input.Descriptor));
        profile.EnrolledAt = DateTimeOffset.UtcNow; profile.EnrolledBy = admin.Id;
        DataStore.Log(db, admin, "Cadastrou biometria", "Reconhecimento facial", $"Colaborador {employee.Registration} — padrão protegido atualizado.");
        return new { employee.Id, employee.Name, enrolled = true, profile.EnrolledAt };
    }));

    [Authorize, HttpDelete("profiles/{employeeId:guid}")]
    public IActionResult Remove(Guid employeeId) => Ok(store.Write(db =>
    {
        var admin = DataStore.Actor(db, ActorId); DataStore.Admin(admin);
        var profile = db.FaceProfiles.FirstOrDefault(f => f.EmployeeId == employeeId)
            ?? throw new DomainError(404, "Cadastro facial não encontrado.");
        db.FaceProfiles.Remove(profile);
        var employee = db.Employees.FirstOrDefault(e => e.Id == employeeId);
        DataStore.Log(db, admin, "Removeu biometria", "Reconhecimento facial", employee?.Registration ?? employeeId.ToString());
        return new { removed = true };
    }));

    // O terminal captura e compara no próprio computador. A rota de batida só aceita
    // conexões locais; imagens e vetores faciais não são enviados ao servidor.
    [AllowAnonymous, HttpPost("terminal/punch")]
    public IActionResult Punch(FaceMatchInput input)
    {
        if (HttpContext.Connection.RemoteIpAddress is not { } ip || !System.Net.IPAddress.IsLoopback(ip))
            return StatusCode(403, new { title = "O terminal facial só pode ser usado no computador local da empresa." });
        if (!ValidDescriptor(input.Descriptor)) return BadRequest(new { title = "Não foi possível reconhecer o rosto. Tente novamente." });
        return Ok(store.Write(db =>
        {
            var candidates = new List<(Staff Staff, double Distance)>();
            foreach (var profile in db.FaceProfiles)
            {
                var employee = db.Employees.FirstOrDefault(e => e.Id == profile.EmployeeId && e.Active && e.WorkMode != "Remoto");
                if (employee is null) continue;
                try
                {
                    var saved = System.Text.Json.JsonSerializer.Deserialize<float[]>(protector.Unprotect(profile.ProtectedDescriptor));
                    if (saved is { Length: 128 }) candidates.Add((employee, Distance(input.Descriptor, saved)));
                }
                catch (System.Security.Cryptography.CryptographicException) { /* Cadastro protegido por outra chave: não autenticar. */ }
            }
            var ranked = candidates.OrderBy(x => x.Distance).Take(2).ToArray();
            if (ranked.Length == 0 || ranked[0].Distance > 0.48 || ranked.Length > 1 && ranked[1].Distance - ranked[0].Distance < 0.08)
                throw new DomainError(401, "Rosto não reconhecido com segurança. Procure o gestor ou registre pelo app.");
            var staff = ranked[0].Staff;
            Require(!db.Vacations.Any(v => v.EmployeeId == staff.Id && v.Status == "Aprovada" && v.Start <= DataStore.Today && v.End >= DataStore.Today), "Colaborador está em férias aprovadas.");
            var day = db.Punches.Where(p => p.EmployeeId == staff.Id && DataStore.Date(p.At) == DataStore.Today).OrderBy(p => p.At).ToList();
            Require(day.Count < Kinds.Length, "A jornada de hoje já foi encerrada.");
            Require(day.Count == 0 || (DateTimeOffset.UtcNow - day[^1].At).TotalSeconds >= 30, "Aguarde 30 segundos antes de registrar outra marcação.");
            var punch = new Punch { EmployeeId = staff.Id, Kind = Kinds[day.Count], At = DateTimeOffset.UtcNow, Source = "Facial local" };
            db.Punches.Add(punch);
            db.Audit.Add(new Change(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.Empty, "Terminal facial", "Registrou", "Ponto", $"{staff.Registration}: {punch.Kind}"));
            return new { employee = staff.Name, registration = staff.Registration, punch.Kind, punch.At, source = punch.Source };
        }));
    }

    private static double Distance(float[] left, float[] right)
    {
        double sum = 0;
        for (int i = 0; i < left.Length; i++) { var delta = left[i] - right[i]; sum += delta * delta; }
        return Math.Sqrt(sum);
    }
}
