/*Dominio AuditLog*/
public class AuditLog
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityName { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public string Details { get; private set; } = string.Empty;

    private AuditLog() { }

    public static AuditLog Create(Guid userId, string action, string entityName,
        string entityId, string details)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Informe o usuário responsável.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(details);

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action.Trim(),
            EntityName = entityName.Trim(),
            EntityId = entityId.Trim(),
            Details = details.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
