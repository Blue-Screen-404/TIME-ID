using TimeId.Operations;
using static TimeId.Services.WorkspaceRules;

namespace TimeId.Services;

public sealed class VacationService(IWorkspaceStore store)
{
    public Leave CreateLeave(Guid actorId, Leave input) => store.Write(db => SaveLeave(db, actorId, input, null));
    public Leave UpdateLeave(Guid actorId, Guid id, Leave input) => store.Write(db => SaveLeave(db, actorId, input, id));
    private Leave SaveLeave(Database db, Guid actorId, Leave input, Guid? id)
    {
        var actor = DataStore.Actor(db, actorId); Own(actor, input.EmployeeId);
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
    public object DeleteLeave(Guid actorId, Guid id) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        var leave = db.Vacations.FirstOrDefault(v => v.Id == id) ?? throw new DomainError(404, "Férias não encontradas.");
        Own(actor, leave.EmployeeId); Require(actor.Role == "Administrador" || leave.Status == "Solicitada", "Apenas solicitações pendentes podem ser canceladas pelo usuário.");
        db.Vacations.Remove(leave); DataStore.Log(db, actor, "Excluiu", "Férias", id.ToString()); return new { deleted = true };
    });
}
