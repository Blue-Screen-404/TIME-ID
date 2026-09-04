/*Dominio Employee*/

public class Employee
{
    public Guid Id { get; set; }

    public string RegistrationNumber { get; set; } // Matrícula
    public string FullName { get; set; }
    public string CPF { get; set; }

    public string Email { get; set; }
    public string Phone { get; set; }

    public DateTime BirthDate { get; set; }
    public DateTime HireDate { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; }

    public Guid PositionId { get; set; }
    public Position Position { get; set; }

    public Guid? LeaderId { get; set; }
    public Employee? Leader { get; set; }

    public EmployeeStatus Status { get; set; }

    public string? Notes { get; set; }
}


