/*Dominio AuditLog*/
public class AuditLog { 
    public Guid Id { get; set; } 

    public Guid UserId { get; set; } 

    public string Action { get; set; } 

    public string EntityName { get; set; }

    public string EntityId { get; set; } 

    public DateTime CreatedAt { get; set; }
     
    public string Details { get; set; } 
}