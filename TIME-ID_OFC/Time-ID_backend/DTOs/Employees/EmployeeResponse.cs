public record EmployeeResponse(
    Guid Id, string RegistrationNumber, string FullName, string CPF,
    string Email, string Phone, DateTime BirthDate, DateTime HireDate,
    Guid DepartmentId, string DepartmentName, Guid PositionId, string PositionName,
    Guid? LeaderId, string? LeaderName, string Status, string? Notes);
