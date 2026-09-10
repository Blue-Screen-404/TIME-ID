/*Dominio TimeRecordEdit*/
public class TimeRecordEdit 
{ 
    public Guid Id { get; set; } 

    public Guid TimeRecordId { get; set; } 
    public TimeRecord TimeRecord { get; set; } 

    public DateTime PreviousDateTime { get; set; } 
    public DateTime NewDateTime { get; set; } 

    public Guid EditedByUserId { get; set; } 

    public string Reason { get; set; } 
    
    public DateTime EditedAt { get; set; } 
}