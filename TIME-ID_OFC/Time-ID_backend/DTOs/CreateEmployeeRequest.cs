public class CreateEmployeeRequest

{
    public string RegistrationNumber { get; set; }
    public string FullName { get; set; }
    public string CPF { get; set; }

    public string Email { get; set; }
    public string Phone { get; set; }

    public DateTime BirthDate { get; set; }
    public DateTime HireDate { get; set; }

    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }

    public Guid? LeaderId { get; set; }
    public string? Notes { get; set; }
}
