using System.ComponentModel.DataAnnotations;

public class DepartmentRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
}

public record DepartmentResponse(Guid Id, string Name, string Description, int EmployeeCount);
