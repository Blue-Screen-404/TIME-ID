public class Vacation 
{ 
    public Guid Id { get; set; } 
    public Guid EmployeeId { get; set; } 
    public Employee Employee { get; set; } 

    public DateTime StartDate { get; set; } 
    public DateTime EndDate { get; set; } 

    public VacationStatus Status { get; set; } 

    public DateTime RequestedAt { get; set; } 
}

public enum VacationStatus 
{ 
    Requested, UnderReview, Approved, Rejected, Completed 
}