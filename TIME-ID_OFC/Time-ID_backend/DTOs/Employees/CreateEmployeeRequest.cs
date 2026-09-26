using System.ComponentModel.DataAnnotations;

public class CreateEmployeeRequest
{
    [Required, MaxLength(30)] public string RegistrationNumber { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(14)] public string CPF { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = string.Empty;
    [MaxLength(30)] public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public DateTime HireDate { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    public Guid? LeaderId { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
}
