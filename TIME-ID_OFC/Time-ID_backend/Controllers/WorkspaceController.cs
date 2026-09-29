using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeId.Operations;

[ApiController, Authorize, Route("api/workspace")]
public class WorkspaceController(DataStore store) : ControllerBase
{
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static Staff StaffById(Database db, Guid id) => db.Employees.FirstOrDefault(e => e.Id == id) ?? throw new DomainError(404, "Funcionário não encontrado.");
    private static void Own(Account actor, Guid employeeId)
    {
        if (actor.Role != "Administrador" && actor.EmployeeId != employeeId) throw new DomainError(403, "Você só pode acessar seus próprios registros.");
    }
    private static void Require(bool valid, string message) { if (!valid) throw new DomainError(400, message); }

    [HttpGet]
    public IActionResult Load() => Ok(store.Read(db =>
    {
        var actor = DataStore.Actor(db, ActorId);
        bool admin = actor.Role == "Administrador";
        var staff = db.Employees.Where(e => admin || e.Id == actor.EmployeeId).ToList();
        var ids = staff.Select(e => e.Id).ToHashSet();
        return new
        {
            employees = staff,
            departments = db.Departments,
            punches = db.Punches.Where(p => ids.Contains(p.EmployeeId)).OrderByDescending(p => p.At),
            vacations = db.Vacations.Where(v => ids.Contains(v.EmployeeId)),
            holidays = db.Holidays.OrderBy(h => h.Date),
            settings = admin ? db.Settings : new Company { Name = db.Settings.Name },
            preferences = new { actor.Theme, actor.Notifications },
            employeeId = actor.EmployeeId,
            twoFactorEnabled = actor.TwoFactorSecret is not null,
            audit = db.Audit.Where(a => admin || a.ActorId == actor.Id).OrderByDescending(a => a.At).Take(100),
            today = DataStore.Today,
            stats = new { total = staff.Count, active = staff.Count(e => e.Active), inactive = staff.Count(e => !e.Active), departments = db.Departments.Count,
                birthdays = staff.Count(e => e.Active && e.BirthDate.Month == DataStore.Today.Month && e.BirthDate.Day == DataStore.Today.Day),
                present = db.Punches.Where(p => ids.Contains(p.EmployeeId) && DataStore.Date(p.At) == DataStore.Today).Select(p => p.EmployeeId).Distinct().Count() }
        };
    }));

    [HttpPost("employees")]
    public IActionResult CreateStaff(StaffInput input) => Ok(store.Write(db => SaveStaff(db, input, null)));
    [HttpPut("employees/{id:guid}")]
    public IActionResult UpdateStaff(Guid id, StaffInput input) => Ok(store.Write(db => SaveStaff(db, input, id)));
    private Staff SaveStaff(Database db, StaffInput input, Guid? id)
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var employee = id is null ? new Staff() : StaffById(db, id.Value);
        string cpf = new(input.Cpf.Where(char.IsAsciiDigit).ToArray());
        Require(ValidCpf(cpf), "Informe um CPF válido.");
        Require(input.Name.Trim().Length >= 2 && input.Position.Trim().Length > 0 && input.Registration.Trim().Length > 0, "Preencha nome, matrícula e cargo.");
        Require(input.BirthDate > new DateOnly(1900, 1, 1) && input.BirthDate < DataStore.Today, "Confira a data de nascimento.");
        Require(input.HireDate >= input.BirthDate && input.HireDate <= DataStore.Today.AddYears(1), "Confira a data de admissão.");
        Require(db.Departments.Any(d => d.Id == input.DepartmentId), "Selecione um departamento cadastrado.");
        Require(!db.Employees.Any(e => e.Id != employee.Id && (e.Cpf == cpf || e.Registration.Equals(input.Registration.Trim(), StringComparison.OrdinalIgnoreCase) || e.Email.Equals(input.Email.Trim(), StringComparison.OrdinalIgnoreCase))), "CPF, matrícula ou e-mail já cadastrado.");
        var linked = db.Accounts.FirstOrDefault(a => a.EmployeeId == employee.Id);
        // As contas de demonstração podem ser vinculadas pelo mesmo e-mail ao primeiro cadastro.
        var matching = db.Accounts.FirstOrDefault(a => a.Email.Equals(input.Email.Trim(), StringComparison.OrdinalIgnoreCase));
        Require(matching is null || matching == linked || matching.EmployeeId is null && linked is null, "E-mail já usado por outra conta.");
        if (linked is null && matching is not null) linked = matching;
        if (linked is not null && linked.Id == actor.Id) Require(input.Active, "Você não pode desativar sua própria conta.");
        if (linked is null && input.CreateAccess)
        {
            Require(!string.IsNullOrWhiteSpace(input.InitialPassword) && input.InitialPassword.Length >= 8, "Informe uma senha inicial com pelo menos 8 caracteres.");
            linked = new Account { Role = input.AccessRole };
            linked.PasswordHash = new PasswordHasher<Account>().HashPassword(linked, input.InitialPassword!);
            db.Accounts.Add(linked);
        }
        Require(input.AccessRole is "Administrador" or "Usuário", "Perfil de acesso inválido.");
        foreach (var team in db.Departments.Where(d => d.LeaderId == employee.Id && (d.Id != input.DepartmentId || !input.Active))) team.LeaderId = null;
        employee.Name = input.Name.Trim(); employee.Cpf = cpf; employee.Registration = input.Registration.Trim();
        employee.Email = input.Email.Trim(); employee.Phone = (input.Phone ?? "").Trim(); employee.BirthDate = input.BirthDate;
        employee.HireDate = input.HireDate; employee.DepartmentId = input.DepartmentId; employee.Position = input.Position.Trim();
        var postalCode = new string((input.PostalCode ?? "").Where(char.IsDigit).ToArray());
        Require(postalCode.Length == 0 || (postalCode.Length == 8 && System.Text.RegularExpressions.Regex.IsMatch(input.PostalCode!, @"^\d{5}-?\d{3}$")), "Informe um CEP com 8 dígitos.");
        var state = (input.State ?? "").Trim().ToUpperInvariant();
        Require(state.Length == 0 || "AC AL AP AM BA CE DF ES GO MA MT MS MG PA PB PR PE PI RJ RN RS RO RR SC SP SE TO".Split(' ').Contains(state), "Selecione uma UF válida.");
        employee.PostalCode = postalCode; employee.Street = (input.Street ?? "").Trim();
        employee.Number = (input.Number ?? "").Trim(); employee.Complement = (input.Complement ?? "").Trim();
        employee.Neighborhood = (input.Neighborhood ?? "").Trim(); employee.City = (input.City ?? "").Trim();
        employee.State = state; employee.Duties = (input.Duties ?? "").Trim();
        employee.WeeklyHours = input.WeeklyHours; employee.Address = (input.Address ?? "").Trim(); employee.Rg = (input.Rg ?? "").Trim(); employee.Active = input.Active;
        if (id is null) db.Employees.Add(employee);
        if (linked is not null) { linked.EmployeeId = employee.Id; linked.Username = employee.Name; linked.Email = employee.Email; linked.Active = employee.Active; }
        DataStore.Log(db, actor, id is null ? "Criou" : "Editou", "Funcionário", employee.Name);
        return employee;
    }
    private static bool ValidCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;
        for (int length = 9; length <= 10; length++)
        {
            int sum = 0;
            for (int i = 0; i < length; i++) sum += (cpf[i] - '0') * (length + 1 - i);
            int digit = sum * 10 % 11; if (digit == 10) digit = 0;
            if (digit != cpf[length] - '0') return false;
        }
        return true;
    }
    [HttpDelete("employees/{id:guid}")]
    public IActionResult DeleteStaff(Guid id) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor); var employee = StaffById(db, id);
        Require(!db.Punches.Any(p => p.EmployeeId == id) && !db.Vacations.Any(v => v.EmployeeId == id), "Há histórico de ponto ou férias. Inative o funcionário para preservar os registros.");
        Require(!db.Accounts.Any(a => a.Id == actor.Id && a.EmployeeId == id), "Você não pode excluir sua própria conta.");
        foreach (var team in db.Departments.Where(d => d.LeaderId == id)) team.LeaderId = null;
        foreach (var account in db.Accounts.Where(a => a.EmployeeId == id)) { account.Active = false; account.EmployeeId = null; }
        db.Employees.Remove(employee); DataStore.Log(db, actor, "Excluiu", "Funcionário", employee.Name); return new { deleted = true };
    }));

    [HttpPost("departments")]
    public IActionResult CreateTeam(Team input) => Ok(store.Write(db => SaveTeam(db, input, null)));
    [HttpPut("departments/{id:guid}")]
    public IActionResult UpdateTeam(Guid id, Team input) => Ok(store.Write(db => SaveTeam(db, input, id)));
    private Team SaveTeam(Database db, Team input, Guid? id)
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var team = id is null ? new Team() : db.Departments.FirstOrDefault(d => d.Id == id) ?? throw new DomainError(404, "Departamento não encontrado.");
        Require(input.Name.Trim().Length >= 2, "Informe o nome do departamento.");
        Require(!db.Departments.Any(d => d.Id != team.Id && d.Name.Equals(input.Name.Trim(), StringComparison.OrdinalIgnoreCase)), "Departamento já cadastrado.");
        if (input.LeaderId is not null) Require(db.Employees.Any(e => e.Id == input.LeaderId && e.Active && e.DepartmentId == team.Id), "O líder deve ser um funcionário ativo deste departamento.");
        team.Name = input.Name.Trim(); team.Description = (input.Description ?? "").Trim(); team.LeaderId = input.LeaderId;
        if (id is null) db.Departments.Add(team);
        DataStore.Log(db, actor, id is null ? "Criou" : "Editou", "Departamento", team.Name); return team;
    }
    [HttpDelete("departments/{id:guid}")]
    public IActionResult DeleteTeam(Guid id) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var team = db.Departments.FirstOrDefault(d => d.Id == id) ?? throw new DomainError(404, "Departamento não encontrado.");
        Require(!db.Employees.Any(e => e.DepartmentId == id), "Transfira os funcionários antes de excluir o departamento.");
        db.Departments.Remove(team); DataStore.Log(db, actor, "Excluiu", "Departamento", team.Name); return new { deleted = true };
    }));
    private static readonly string[] Kinds = { "Entrada", "Início do intervalo", "Retorno do intervalo", "Saída" };
    [HttpPost("punches")]
    public IActionResult RegisterPunch(PunchInput input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); Own(actor, input.EmployeeId);
        var staff = StaffById(db, input.EmployeeId); Require(staff.Active, "Funcionário inativo não pode registrar ponto.");
        Require(!db.Vacations.Any(v => v.EmployeeId == staff.Id && v.Status == "Aprovada" && v.Start <= DataStore.Today && v.End >= DataStore.Today), "Funcionário está em férias aprovadas.");
        var day = db.Punches.Where(p => p.EmployeeId == staff.Id && DataStore.Date(p.At) == DataStore.Today).OrderBy(p => p.At).ToList();
        Require(day.Count < 4, "A jornada de hoje já foi encerrada.");
        Require(day.Count == 0 || (DateTimeOffset.UtcNow - day[^1].At).TotalSeconds >= 30, "Aguarde 30 segundos antes de registrar outra marcação.");
        var punch = new Punch { EmployeeId = staff.Id, Kind = Kinds[day.Count], At = DateTimeOffset.UtcNow };
        db.Punches.Add(punch); DataStore.Log(db, actor, "Registrou", "Ponto", $"{staff.Name}: {punch.Kind}"); return punch;
    }));
    [HttpPut("punches/{id:guid}")]
    public IActionResult CorrectPunch(Guid id, PunchCorrection input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var punch = db.Punches.FirstOrDefault(p => p.Id == id) ?? throw new DomainError(404, "Marcação não encontrada.");
        Require(input.Reason.Trim().Length >= 5, "Informe a justificativa da correção.");
        Require(input.At <= DateTimeOffset.UtcNow && DataStore.Date(input.At) == DataStore.Date(punch.At), "Mantenha a data original e não use um horário futuro.");
        var siblings = db.Punches.Where(p => p.EmployeeId == punch.EmployeeId && DataStore.Date(p.At) == DataStore.Date(punch.At)).OrderBy(p => p.At).ToList();
        int index = siblings.FindIndex(p => p.Id == id);
        Require((index == 0 || siblings[index - 1].At < input.At) && (index == siblings.Count - 1 || siblings[index + 1].At > input.At), "A correção deve respeitar a sequência das marcações.");
        var old = punch.At; punch.At = input.At; punch.Correction = input.Reason.Trim();
        DataStore.Log(db, actor, "Corrigiu", "Ponto", $"{id}: {old:O} → {punch.At:O}. Motivo: {punch.Correction}"); return punch;
    }));
    [HttpDelete("punches/{id:guid}")]
    public IActionResult DeletePunch(Guid id, [FromQuery] string reason) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var punch = db.Punches.FirstOrDefault(p => p.Id == id) ?? throw new DomainError(404, "Marcação não encontrada.");
        Require(!string.IsNullOrWhiteSpace(reason) && reason.Trim().Length >= 5 && reason.Length <= 500, "Informe uma justificativa entre 5 e 500 caracteres.");
        Require(!db.Punches.Any(p => p.EmployeeId == punch.EmployeeId && DataStore.Date(p.At) == DataStore.Date(punch.At) && p.At > punch.At), "Exclua apenas a última marcação do dia para preservar a sequência.");
        db.Punches.Remove(punch); DataStore.Log(db, actor, "Excluiu", "Ponto", $"{id}: {punch.At:O}. Motivo: {reason.Trim()}"); return new { deleted = true };
    }));
    [HttpPost("vacations")]
    public IActionResult CreateLeave(Leave input) => Ok(store.Write(db => SaveLeave(db, input, null)));
    [HttpPut("vacations/{id:guid}")]
    public IActionResult UpdateLeave(Guid id, Leave input) => Ok(store.Write(db => SaveLeave(db, input, id)));
    private Leave SaveLeave(Database db, Leave input, Guid? id)
    {
        var actor = DataStore.Actor(db, ActorId); Own(actor, input.EmployeeId);
        var staff = StaffById(db, input.EmployeeId); Require(staff.Active, "Funcionário inativo.");
        Require(input.Status is "Solicitada" or "Aprovada" or "Recusada", "Status inválido.");
        var leave = id is null ? new Leave() : db.Vacations.FirstOrDefault(v => v.Id == id) ?? throw new DomainError(404, "Férias não encontradas.");
        if (id is not null) Own(actor, leave.EmployeeId);
        if (actor.Role != "Administrador") Require(input.Status == "Solicitada" && leave.Status == "Solicitada", "Somente administradores podem decidir ou alterar férias já analisadas.");
        Require(input.Start >= staff.HireDate && input.Start >= new DateOnly(2000, 1, 1) && input.End >= input.Start && input.End.DayNumber - input.Start.DayNumber < 14, "As férias devem ter de 1 a 14 dias corridos por solicitação, após a admissão (incluindo início e fim).");
        Require(input.Status == "Recusada" || !db.Vacations.Any(v => v.Id != leave.Id && v.EmployeeId == input.EmployeeId && v.Status != "Recusada" && v.Start <= input.End && v.End >= input.Start), "Já existem férias solicitadas ou aprovadas neste período.");
        leave.EmployeeId = input.EmployeeId; leave.Start = input.Start; leave.End = input.End; leave.Status = input.Status; leave.Notes = (input.Notes ?? "").Trim();
        if (id is null) db.Vacations.Add(leave);
        DataStore.Log(db, actor, id is null ? "Solicitou" : "Editou", "Férias", $"{staff.Name}: {leave.Start} a {leave.End} — {leave.Status}"); return leave;
    }
    [HttpDelete("vacations/{id:guid}")]
    public IActionResult DeleteLeave(Guid id) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId);
        var leave = db.Vacations.FirstOrDefault(v => v.Id == id) ?? throw new DomainError(404, "Férias não encontradas.");
        Own(actor, leave.EmployeeId); Require(actor.Role == "Administrador" || leave.Status == "Solicitada", "Apenas solicitações pendentes podem ser canceladas pelo usuário.");
        db.Vacations.Remove(leave); DataStore.Log(db, actor, "Excluiu", "Férias", id.ToString()); return new { deleted = true };
    }));
    [HttpPost("holidays")]
    public IActionResult CreateHoliday(DayOff input) => Ok(store.Write(db => SaveHoliday(db, input, null)));
    [HttpPut("holidays/{id:guid}")]
    public IActionResult UpdateHoliday(Guid id, DayOff input) => Ok(store.Write(db => SaveHoliday(db, input, id)));
    private DayOff SaveHoliday(Database db, DayOff input, Guid? id)
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var holiday = id is null ? new DayOff() : db.Holidays.FirstOrDefault(h => h.Id == id) ?? throw new DomainError(404, "Feriado não encontrado.");
        Require(input.Date.Year >= 2000 && input.Date.Year <= 2100 && input.Name.Trim().Length > 0, "Informe nome e data válidos.");
        Require(!db.Holidays.Any(h => h.Id != holiday.Id && h.Date == input.Date && h.Name.Equals(input.Name.Trim(), StringComparison.OrdinalIgnoreCase)), "Feriado já cadastrado.");
        Require(input.Scope is "Nacional" or "Estadual" or "Municipal" or "Empresa", "Abrangência inválida.");
        holiday.Name = input.Name.Trim(); holiday.Date = input.Date; holiday.Scope = input.Scope;
        if (id is null) db.Holidays.Add(holiday);
        DataStore.Log(db, actor, id is null ? "Criou" : "Editou", "Feriado", holiday.Name); return holiday;
    }
    [HttpDelete("holidays/{id:guid}")]
    public IActionResult DeleteHoliday(Guid id) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var holiday = db.Holidays.FirstOrDefault(h => h.Id == id) ?? throw new DomainError(404, "Feriado não encontrado.");
        db.Holidays.Remove(holiday); DataStore.Log(db, actor, "Excluiu", "Feriado", holiday.Name); return new { deleted = true };
    }));
    [HttpPut("settings")]
    public IActionResult SaveSettings(Company input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor); Require(input.Name.Trim().Length > 0, "Informe o nome da empresa.");
        db.Settings = input; DataStore.Log(db, actor, "Editou", "Empresa", input.Name); return input;
    }));
    [HttpPut("preferences")]
    public IActionResult SavePreferences(Preferences input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); Require(input.Theme is "claro" or "escuro", "Tema inválido."); actor.Theme = input.Theme; actor.Notifications = input.Notifications; return input;
    }));
    [HttpPut("password")]
    public IActionResult Password(PasswordChange input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); var hasher = new PasswordHasher<Account>();
        Require(hasher.VerifyHashedPassword(actor, actor.PasswordHash, input.CurrentPassword) != PasswordVerificationResult.Failed, "Senha atual incorreta.");
        Require(input.NewPassword != input.CurrentPassword, "Escolha uma senha diferente da atual.");
        actor.PasswordHash = hasher.HashPassword(actor, input.NewPassword); actor.SecurityStamp = Guid.NewGuid().ToString();
        DataStore.Log(db, actor, "Alterou", "Senha", "Senha alterada; sessões invalidadas."); return new { changed = true };
    }));
    private static void CheckPassword(Account actor, string password)
    {
        Require(new PasswordHasher<Account>().VerifyHashedPassword(actor, actor.PasswordHash, password) != PasswordVerificationResult.Failed, "Senha atual incorreta.");
    }
    [HttpPost("two-factor/setup")]
    public IActionResult SetupTwoFactor(TwoFactorInput input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); CheckPassword(actor, input.Password);
        Require(actor.TwoFactorSecret is null, "A autenticação em dois fatores já está ativa.");
        actor.PendingTwoFactorSecret = TwoFactor.NewSecret();
        return new { secret = actor.PendingTwoFactorSecret };
    }));
    [HttpPost("two-factor/enable")]
    public IActionResult EnableTwoFactor(TwoFactorInput input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); CheckPassword(actor, input.Password);
        Require(actor.TwoFactorSecret is null && actor.PendingTwoFactorSecret is not null && TwoFactor.Verify(actor.PendingTwoFactorSecret, input.Code), "Código inválido. Confira o autenticador.");
        actor.TwoFactorSecret = actor.PendingTwoFactorSecret; actor.PendingTwoFactorSecret = null;
        var codes = Enumerable.Range(0, 8).Select(_ => Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))).ToArray();
        actor.RecoveryHashes = codes.Select(TwoFactor.RecoveryHash).ToList();
        DataStore.Log(db, actor, "Ativou", "Dois fatores", "Autenticação TOTP ativada.");
        return new { recoveryCodes = codes };
    }));
    [HttpPost("two-factor/disable")]
    public IActionResult DisableTwoFactor(TwoFactorInput input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); CheckPassword(actor, input.Password);
        Require(actor.TwoFactorSecret is not null && TwoFactor.VerifyAccount(actor, input.Code), "Informe um código válido do autenticador ou de recuperação.");
        actor.TwoFactorSecret = null; actor.PendingTwoFactorSecret = null; actor.RecoveryHashes.Clear();
        DataStore.Log(db, actor, "Desativou", "Dois fatores", "Autenticação TOTP desativada."); return new { disabled = true };
    }));
    [HttpGet("backup")]
    public IActionResult Backup()
    {
        var snapshot = store.Write(db => { var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor); DataStore.Log(db, actor, "Exportou", "Backup", "Cópia completa dos dados locais."); return db; });
        return File(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(snapshot, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web) { WriteIndented = true }), "application/json", $"timeid-backup-{DataStore.Today}.json");
    }
    [HttpGet("reports")]
    public IActionResult Report([FromQuery] string type, [FromQuery] DateOnly start, [FromQuery] DateOnly end, [FromQuery] bool csv = false, [FromQuery] string? fields = null)
    {
        var report = store.Read(db =>
        {
            var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
            Require(start.Year >= 2000 && end >= start && end.DayNumber - start.DayNumber <= 3660, "Informe um período válido de até dez anos.");
            string Department(Guid id) => db.Departments.FirstOrDefault(d => d.Id == id)?.Name ?? "—";
            string Employee(Guid id) => db.Employees.FirstOrDefault(e => e.Id == id)?.Name ?? "Removido";
            string[] columns; IEnumerable<string[]> rows;
            switch (type)
            {
                case "custom":
                case "employees":
                    columns = new[] { "Matrícula", "Nome", "Departamento", "Cargo", "Admissão", "Status" };
                    rows = db.Employees.Where(e => e.HireDate >= start && e.HireDate <= end).Select(e => new[] { e.Registration, e.Name, Department(e.DepartmentId), e.Position, e.HireDate.ToString("dd/MM/yyyy"), e.Active ? "Ativo" : "Inativo" }); break;
                case "departments":
                    columns = new[] { "Departamento", "Líder", "Funcionários", "Ativos" };
                    rows = db.Departments.Select(d => new[] { d.Name, d.LeaderId is Guid leader ? Employee(leader) : "—", db.Employees.Count(e => e.DepartmentId == d.Id).ToString(), db.Employees.Count(e => e.DepartmentId == d.Id && e.Active).ToString() }); break;
                case "vacations":
                    columns = new[] { "Funcionário", "Início", "Fim", "Dias", "Status" };
                    rows = db.Vacations.Where(v => v.Start <= end && v.End >= start).Select(v => new[] { Employee(v.EmployeeId), v.Start.ToString("dd/MM/yyyy"), v.End.ToString("dd/MM/yyyy"), (v.End.DayNumber - v.Start.DayNumber + 1).ToString(), v.Status }); break;
                case "holidays":
                    columns = new[] { "Feriado", "Data", "Abrangência" };
                    rows = db.Holidays.Where(h => h.Date >= start && h.Date <= end).Select(h => new[] { h.Name, h.Date.ToString("dd/MM/yyyy"), h.Scope }); break;
                case "punches":
                    columns = new[] { "Funcionário", "Data", "Marcação", "Horário", "Justificativa" };
                    rows = db.Punches.Where(p => DataStore.Date(p.At) >= start && DataStore.Date(p.At) <= end).OrderBy(p => p.At).Select(p => new[] { Employee(p.EmployeeId), DataStore.Date(p.At).ToString("dd/MM/yyyy"), p.Kind, p.At.ToOffset(TimeSpan.FromHours(-3)).ToString("HH:mm:ss"), p.Correction ?? "" }); break;
                default: throw new DomainError(400, "Tipo de relatório inválido.");
            }
            if (type == "custom")
            {
                var selected = (fields ?? "0,1,2,3,4,5").Split(',');
                Require(selected.Length is > 0 and <= 6 && selected.All(s => int.TryParse(s, out int i) && i >= 0 && i < columns.Length), "Selecione colunas válidas.");
                var indexes = selected.Select(int.Parse).Distinct().ToArray();
                columns = indexes.Select(i => columns[i]).ToArray();
                rows = rows.Select(row => indexes.Select(i => row[i]).ToArray());
            }
            return new { columns, rows = rows.ToArray() };
        });
        if (!csv) return Ok(report);
        // Protege planilhas contra fórmulas injetadas em campos de texto.
        string Escape(string value) { if (value.TrimStart().StartsWith('=') || value.TrimStart().StartsWith('+') || value.TrimStart().StartsWith('-') || value.TrimStart().StartsWith('@')) value = "'" + value; return "\"" + value.Replace("\"", "\"\"") + "\""; }
        var text = string.Join(";", report.columns.Select(Escape)) + "\r\n" + string.Join("\r\n", report.rows.Select(r => string.Join(";", r.Select(Escape))));
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(text)).ToArray(), "text/csv; charset=utf-8", $"timeid-{type}-{start}-{end}.csv");
    }
}
