/*Dominio User*/
public class User 
{
    public Guid Id { get; set; } 

    public string Username { get; set; } 

    public string Email { get; set; } 

    public string PasswordHash { get; set; } 

    public Guid EmployeeId { get; set; } 

    public Employee Employee { get; set; } 
    
    public bool IsActive { get; set; }
}