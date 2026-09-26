public class DashboardService
{
    private readonly InMemoryEmployeeRepository employees;

    public DashboardService(InMemoryEmployeeRepository employees) => this.employees = employees;

    public DashboardResponse GetMetrics()
    {
        var all = employees.GetAll();
        return new DashboardResponse(
            all.Count(e => e.Status == EmployeeStatus.Active),
            all.Count(e => e.Status == EmployeeStatus.Inactive));
    }
}
