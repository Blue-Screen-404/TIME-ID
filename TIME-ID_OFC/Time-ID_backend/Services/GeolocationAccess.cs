using TimeId.Operations;
namespace TimeId.Services;

public static class GeolocationAccess
{
    public static bool Leads(Database db, Account actor, Guid departmentId) => actor.Role == "Gestor" && actor.EmployeeId is Guid employeeId &&
        db.Employees.Any(e => e.Id == employeeId && e.Active) && db.Departments.Any(d => d.Id == departmentId && d.LeaderId == employeeId);

    public static bool CanManage(Database db, Account actor, WorkLocation location) => actor.Role == "Administrador" ||
        (db.GeolocationSettings.ManagersCanManageLocations && actor.Role == "Gestor" && !location.CompanyWide &&
        location.DepartmentIds.Count + location.EmployeeIds.Count > 0 &&
        location.DepartmentIds.All(id => Leads(db, actor, id)) &&
        location.EmployeeIds.All(id => db.Employees.Any(e => e.Id == id && Leads(db, actor, e.DepartmentId))));

    public static bool CanReview(Database db, Account actor, Guid employeeId) => actor.Role == "Administrador" ||
        (db.GeolocationSettings.ManagersCanReview && db.Employees.Any(e => e.Id == employeeId && Leads(db, actor, e.DepartmentId)));

    public static void Require(bool allowed) { if (!allowed) throw new DomainError(403, "Acesso não autorizado para esta abrangência."); }
}
