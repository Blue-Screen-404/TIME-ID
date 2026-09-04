/*Dominio TimeRecord*/
public class TimeRecord 
{ 
    public Guid Id { get; set; } 

    public Guid EmployeeId { get; set; } 

    public Employee Employee { get; set; } 

    public DateTime DateTime { get; set; } 

    public TimeRecordType Type { get; set; }

    public bool IsManual { get; set; } 
}

public enum TimeRecordType 
{ 
    Entry, BreakStart, BreakEnd, Exit 
}