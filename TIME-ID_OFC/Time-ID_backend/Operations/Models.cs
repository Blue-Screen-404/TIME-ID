using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TimeId.Operations;

public class Database
{
    public int Version { get; set; } = 1;
    public List<Account> Accounts { get; set; } = new();
    public List<Staff> Employees { get; set; } = new();
    public List<Team> Departments { get; set; } = new();
    public List<Punch> Punches { get; set; } = new();
    public List<Leave> Vacations { get; set; } = new();
    public List<DayOff> Holidays { get; set; } = new();
    public List<Change> Audit { get; set; } = new();
    public Company Settings { get; set; } = new();
}
public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Usuário";
    public bool Active { get; set; } = true;
    public string? PhotoUrl { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? TwoFactorSecret { get; set; }
    public string? PendingTwoFactorSecret { get; set; }
    public List<string> RecoveryHashes { get; set; } = new();
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
    public string Theme { get; set; } = "claro";
    public bool Notifications { get; set; } = true;
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class StaffInput
{
    [Required, StringLength(120, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required, StringLength(30)] public string Registration { get; set; } = "";
    [Required, StringLength(14)] public string Cpf { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [StringLength(25)] public string Phone { get; set; } = "";
    public DateOnly BirthDate { get; set; }
    public DateOnly HireDate { get; set; }
    public Guid DepartmentId { get; set; }
    [Required, StringLength(100)] public string Position { get; set; } = "";
    [Range(1, 44)] public int WeeklyHours { get; set; } = 40;
    [StringLength(300)] public string Address { get; set; } = "";
    [StringLength(9)] public string PostalCode { get; set; } = "";
    [StringLength(150)] public string Street { get; set; } = "";
    [StringLength(20)] public string Number { get; set; } = "";
    [StringLength(100)] public string Complement { get; set; } = "";
    [StringLength(100)] public string Neighborhood { get; set; } = "";
    [StringLength(100)] public string City { get; set; } = "";
    [StringLength(2)] public string State { get; set; } = "";
    [StringLength(500)] public string Duties { get; set; } = "";
    [StringLength(25)] public string Rg { get; set; } = "";
    public bool Active { get; set; } = true;
    public bool CreateAccess { get; set; }
    [StringLength(128, MinimumLength = 8)] public string? InitialPassword { get; set; }
    [RegularExpression("^(Administrador|Usuário)$")] public string AccessRole { get; set; } = "Usuário";
}
public class Staff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Registration { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateOnly BirthDate { get; set; }
    public DateOnly HireDate { get; set; }
    public Guid DepartmentId { get; set; }
    public string Position { get; set; } = "";
    public int WeeklyHours { get; set; }
    public string Address { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Street { get; set; } = "";
    public string Number { get; set; } = "";
    public string Complement { get; set; } = "";
    public string Neighborhood { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Duties { get; set; } = "";
    public string Rg { get; set; } = "";
    public bool Active { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, StringLength(100, MinimumLength = 2)] public string Name { get; set; } = "";
    [StringLength(400)] public string Description { get; set; } = "";
    public Guid? LeaderId { get; set; }
}
public class Punch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public string Kind { get; set; } = "Entrada";
    public DateTimeOffset At { get; set; }
    public string? Correction { get; set; }
}
public class PunchInput { public Guid EmployeeId { get; set; } }
public class PunchCorrection
{
    public DateTimeOffset At { get; set; }
    [Required, StringLength(500, MinimumLength = 5)] public string Reason { get; set; } = "";
}
public class Leave
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    [RegularExpression("^(Solicitada|Aprovada|Recusada)$")] public string Status { get; set; } = "Solicitada";
    [StringLength(500)] public string Notes { get; set; } = "";
}
public class DayOff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, StringLength(120)] public string Name { get; set; } = "";
    public DateOnly Date { get; set; }
    [RegularExpression("^(Nacional|Estadual|Municipal|Empresa)$")] public string Scope { get; set; } = "Nacional";
}
public class Company
{
    [Required, StringLength(120)] public string Name { get; set; } = "TIMEID";
    [StringLength(18)] public string Cnpj { get; set; } = "";
    [EmailAddress, StringLength(254)] public string Email { get; set; } = "contato@timeid.local";
    [StringLength(30)] public string Phone { get; set; } = "";
    [StringLength(300)] public string Address { get; set; } = "";
}
public record Change(Guid Id, DateTimeOffset At, Guid ActorId, string Actor, string Action, string Entity, string Detail);
public class Preferences
{
    [RegularExpression("^(claro|escuro)$")] public string Theme { get; set; } = "claro";
    public bool Notifications { get; set; } = true;
}
public class PasswordChange
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 8)] public string NewPassword { get; set; } = "";
}
public class DomainError(int status, string message) : Exception(message) { public int Status { get; } = status; }

public class TwoFactorInput
{
    [Required] public string Password { get; set; } = "";
    [StringLength(64)] public string? Code { get; set; }
}
