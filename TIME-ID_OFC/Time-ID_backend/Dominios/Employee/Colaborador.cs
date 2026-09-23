/*Dominio Employee*/

public class Employee
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public void UpdateContact(string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("O e-mail não pode ficar vazio.");
        }

        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
    }

    

    public required string RegistrationNumber { get; set; } // Matrícula
    public required string FullName { get; set; }
    public required string CPF { get; set; }

    public DateTime BirthDate { get; set; }
    public DateTime HireDate { get; set; }

    public Guid DepartmentId { get; set; }
    public required Department Department { get; set; }

    public Guid PositionId { get; set; }
    public required Position Position { get; set; }

    public Guid? LeaderId { get; set; }
    public Employee? Leader { get; set; }

    public EmployeeStatus Status { get; private set; }

    public WorkSchedule? WorkSchedule { get; private set; }

    public void AssignWorkSchedule(WorkSchedule workSchedule)
    {
        ArgumentNullException.ThrowIfNull(workSchedule);
        WorkSchedule = workSchedule;
    }

    public string? Notes { get; set; }

    public void ChangeDepartment(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);
        Department?.Employees?.Remove(this);
        Department = department;
        DepartmentId = department.Id;
        department.Employees ??= new List<Employee>();
        if (!department.Employees.Contains(this))
        {
            department.Employees.Add(this);
        }
    }

    public void ChangePosition(Position position)
    {
        ArgumentNullException.ThrowIfNull(position);
        Position = position;
        PositionId = position.Id;
    }

    public void AssignLeader(Employee leader)
    {
        ArgumentNullException.ThrowIfNull(leader);
        if (ReferenceEquals(this, leader) || (Id != Guid.Empty && Id == leader.Id))
        {
            throw new ArgumentException("O colaborador não pode ser seu próprio líder.", nameof(leader));
        }

        Leader = leader;
        LeaderId = leader.Id;
    }

    public void Activate()
    {
        Status = EmployeeStatus.Active;
    }

    public void Deactivate()
    {
        Status = EmployeeStatus.Inactive;
    }
}

public enum EmployeeStatus
{
    Active,
    Inactive
}


