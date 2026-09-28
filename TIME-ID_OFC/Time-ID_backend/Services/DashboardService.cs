public sealed class DashboardService
{
    private readonly InMemoryEmployeeRepository employees;
    private readonly InMemoryDepartmentRepository departments;

    public DashboardService(InMemoryEmployeeRepository employees,
        InMemoryDepartmentRepository departments)
    {
        this.employees = employees;
        this.departments = departments;
    }

    public DashboardResponse GetMetrics()
    {
        List<Employee> all = employees.GetAll();
        DateTime today = DateTime.Today;
        int birthdaysToday = all.Count(employee => employee.Status == EmployeeStatus.Active &&
            employee.BirthDate.Month == today.Month && employee.BirthDate.Day == today.Day);
        List<EmployeeResponse> recent = all.OrderByDescending(employee => employee.HireDate)
            .Take(5).Select(employee => new EmployeeResponse(employee.Id,
                employee.RegistrationNumber, employee.FullName, employee.CPF,
                employee.Email, employee.Phone, employee.BirthDate, employee.HireDate,
                employee.DepartmentId, employee.Department.Name, employee.PositionId,
                employee.Position.Name, employee.LeaderId, employee.Leader?.FullName,
                employee.Status.ToString())).ToList();

        return new DashboardResponse(
            all.Count(employee => employee.Status == EmployeeStatus.Active),
            all.Count(employee => employee.Status == EmployeeStatus.Inactive),
            departments.GetAll().Count,
            birthdaysToday,
            recent);
    }
}
