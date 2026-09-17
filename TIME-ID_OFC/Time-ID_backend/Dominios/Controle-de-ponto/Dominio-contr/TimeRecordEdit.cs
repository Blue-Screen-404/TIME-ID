/*Dominio TimeRecordEdit*/
public class TimeRecordEdit 
{ 
    public Guid Id { get; private set; }

    public Guid TimeRecordId { get; private set; }
    public TimeRecord TimeRecord { get; private set; } = null!;

    public DateTime PreviousDateTime { get; private set; }
    public DateTime NewDateTime { get; private set; }

    public Guid EditedByUserId { get; private set; }

    public string Reason { get; private set; } = string.Empty;
    
    public DateTime EditedAt { get; private set; }

    private TimeRecordEdit() { }

    public static TimeRecordEdit Create(TimeRecord record, DateTime previousDateTime,
        DateTime newDateTime, Guid userId, string reason)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (record.Id == Guid.Empty || userId == Guid.Empty || newDateTime == default ||
            previousDateTime == default || previousDateTime != record.DateTime ||
            previousDateTime == newDateTime)
        {
            throw new ArgumentException("Informe registro, usuário e horários válidos para a correção.");
        }

        return new TimeRecordEdit
        {
            Id = Guid.NewGuid(),
            TimeRecord = record,
            TimeRecordId = record.Id,
            PreviousDateTime = previousDateTime,
            NewDateTime = newDateTime,
            EditedByUserId = userId,
            Reason = reason.Trim(),
            EditedAt = DateTime.UtcNow
        };
    }
}
