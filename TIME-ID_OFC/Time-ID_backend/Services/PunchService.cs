using TimeId.Operations;
using static TimeId.Services.WorkspaceRules;

namespace TimeId.Services;

public sealed class PunchService(IWorkspaceStore store)
{
    private static readonly string[] Kinds = { "Entrada", "Início do intervalo", "Retorno do intervalo", "Saída" };
    public Punch RegisterPunch(Guid actorId, PunchInput input) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId); Own(actor, input.EmployeeId);
        var staff = StaffById(db, input.EmployeeId); Require(staff.Active, "Funcionário inativo não pode registrar ponto.");
        Require(!db.Vacations.Any(v => v.EmployeeId == staff.Id && v.Status == "Aprovada" && v.Start <= DataStore.Today && v.End >= DataStore.Today), "Funcionário está em férias aprovadas.");
        var day = db.Punches.Where(p => p.EmployeeId == staff.Id && DataStore.Date(p.At) == DataStore.Today).OrderBy(p => p.At).ToList();
        Require(day.Count < 4, "A jornada de hoje já foi encerrada.");
        Require(day.Count == 0 || (DateTimeOffset.UtcNow - day[^1].At).TotalSeconds >= 30, "Aguarde 30 segundos antes de registrar outra marcação.");
        var punch = new Punch { EmployeeId = staff.Id, Kind = Kinds[day.Count], At = DateTimeOffset.UtcNow, Source = "App" };
        db.Punches.Add(punch); DataStore.Log(db, actor, "Registrou", "Ponto", $"{staff.Name}: {punch.Kind}"); return punch;
    });
    public Punch CorrectPunch(Guid actorId, Guid id, PunchCorrection input) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId); DataStore.Admin(actor);
        var punch = db.Punches.FirstOrDefault(p => p.Id == id) ?? throw new DomainError(404, "Marcação não encontrada.");
        Require(input.Reason.Trim().Length >= 5, "Informe a justificativa da correção.");
        Require(input.At <= DateTimeOffset.UtcNow && DataStore.Date(input.At) == DataStore.Date(punch.At), "Mantenha a data original e não use um horário futuro.");
        var siblings = db.Punches.Where(p => p.EmployeeId == punch.EmployeeId && DataStore.Date(p.At) == DataStore.Date(punch.At)).OrderBy(p => p.At).ToList();
        int index = siblings.FindIndex(p => p.Id == id);
        Require((index == 0 || siblings[index - 1].At < input.At) && (index == siblings.Count - 1 || siblings[index + 1].At > input.At), "A correção deve respeitar a sequência das marcações.");
        var old = punch.At; punch.At = input.At; punch.Correction = input.Reason.Trim();
        DataStore.Log(db, actor, "Corrigiu", "Ponto", $"{id}: {old:O} → {punch.At:O}. Motivo: {punch.Correction}"); return punch;
    });
    public object DeletePunch(Guid actorId, Guid id, string reason) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId); DataStore.Admin(actor);
        var punch = db.Punches.FirstOrDefault(p => p.Id == id) ?? throw new DomainError(404, "Marcação não encontrada.");
        Require(!string.IsNullOrWhiteSpace(reason) && reason.Trim().Length >= 5 && reason.Length <= 500, "Informe uma justificativa entre 5 e 500 caracteres.");
        Require(!db.Punches.Any(p => p.EmployeeId == punch.EmployeeId && DataStore.Date(p.At) == DataStore.Date(punch.At) && p.At > punch.At), "Exclua apenas a última marcação do dia para preservar a sequência.");
        db.Punches.Remove(punch); DataStore.Log(db, actor, "Excluiu", "Ponto", $"{id}: {punch.At:O}. Motivo: {reason.Trim()}"); return new { deleted = true };
    });
}
