/*Dominio User*/
public class User 
{
    public Guid Id { get; set; } 

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public Guid? EmployeeId { get; set; }

    public Employee? Employee { get; set; }
    
    public bool IsActive { get; private set; }

    public Role? Role { get; private set; }
    public Guid? RoleId => Role?.Id;

    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);
        if (role.Id == Guid.Empty)
        {
            throw new ArgumentException("O perfil precisa ter um identificador.", nameof(role));
        }

        Role = role;
    }

    public bool HasPermission(Guid permissionId)
    {
        return IsActive && (Role?.HasPermission(permissionId) ?? false);
    }

    public void UpdateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        email = email.Trim();
        if (!System.Net.Mail.MailAddress.TryCreate(email, out var address) || address.Address != email)
        {
            throw new ArgumentException("E-mail inválido.", nameof(email));
        }

        Email = email;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
