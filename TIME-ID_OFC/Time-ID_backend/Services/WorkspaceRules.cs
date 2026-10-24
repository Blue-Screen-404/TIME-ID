using TimeId.Operations;

namespace TimeId.Services;

internal static class WorkspaceRules
{
    internal static Staff StaffById(Database db, Guid id) => db.Employees.FirstOrDefault(e => e.Id == id) ?? throw new DomainError(404, "Funcionário não encontrado.");
    internal static void Own(Account actor, Guid employeeId)
    {
        if (actor.Role != "Administrador" && actor.EmployeeId != employeeId) throw new DomainError(403, "Você só pode acessar seus próprios registros.");
    }
    internal static void Require(bool valid, string message) { if (!valid) throw new DomainError(400, message); }
}
