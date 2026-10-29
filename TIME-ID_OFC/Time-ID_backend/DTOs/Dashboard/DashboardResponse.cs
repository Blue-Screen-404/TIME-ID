public record DashboardResponse(
    int ActiveEmployees,
    int InactiveEmployees,
    int DepartmentCount,
    int BirthdaysToday,
    IReadOnlyList<EmployeeResponse> RecentEmployees);
