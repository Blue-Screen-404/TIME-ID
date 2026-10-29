using TimeId.Operations;

public sealed class DashboardService(DataStore store, EmployeeService employees)
{
    public DashboardResponse GetMetrics(Guid actorId)
    {
        var visibleEmployees = employees.GetAll(actorId, null, null);
        bool administrator = store.Read(db => DataStore.Actor(db, actorId).Role == "Administrador");
        var today = DataStore.Today;
        int departmentCount = store.Read(db => administrator
            ? db.Departments.Count
            : visibleEmployees.Select(employee => employee.DepartmentId).Distinct().Count());
        return new DashboardResponse(
            visibleEmployees.Count(employee => employee.Status == EmployeeStatus.Active.ToString()),
            visibleEmployees.Count(employee => employee.Status == EmployeeStatus.Inactive.ToString()),
            departmentCount,
            visibleEmployees.Count(employee => employee.Status == EmployeeStatus.Active.ToString() &&
                employee.BirthDate.Month == today.Month && employee.BirthDate.Day == today.Day),
            visibleEmployees.OrderByDescending(employee => employee.HireDate).Take(5).ToList());
    }
}
