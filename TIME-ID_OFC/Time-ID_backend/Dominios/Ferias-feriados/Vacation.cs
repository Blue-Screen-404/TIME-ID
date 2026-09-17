/*Dominio Vacation*/
public class Vacation 
{ 
    public Guid Id { get; private set; } 
    public Guid EmployeeId { get; private set; } 
    public Employee Employee { get; private set; } = null!;

    public DateTime StartDate { get; private set; } 
    public DateTime EndDate { get; private set; } 

    public VacationStatus Status { get; private set; } 

    public DateTime RequestedAt { get; private set; }

    private Vacation() { }

    // Passe todas as solicitações existentes, inclusive as ainda pendentes.
    public static Vacation Request(Employee employee, DateTime startDate, DateTime endDate,
        IEnumerable<Vacation> existingVacations)
    {
        ArgumentNullException.ThrowIfNull(employee);
        if (employee.Id == Guid.Empty || startDate == default || endDate == default ||
            endDate.Date < startDate.Date)
        {
            throw new ArgumentException("Informe colaborador e período válidos.");
        }

        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            Employee = employee,
            EmployeeId = employee.Id,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Status = VacationStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        vacation.EnsureNoOverlap(existingVacations);
        return vacation;
    }

    public void StartReview()
    {
        if (Status != VacationStatus.Requested)
        {
            throw new InvalidOperationException("Somente solicitações pendentes podem entrar em análise.");
        }

        Status = VacationStatus.UnderReview;
    }

    public void Approve(IEnumerable<Vacation> existingVacations)
    {
        if (Status != VacationStatus.UnderReview)
        {
            throw new InvalidOperationException("A solicitação precisa estar em análise.");
        }

        EnsureNoOverlap(existingVacations);
        Status = VacationStatus.Approved;
    }

    public void Reject()
    {
        if (Status != VacationStatus.UnderReview)
        {
            throw new InvalidOperationException("A solicitação precisa estar em análise.");
        }

        Status = VacationStatus.Rejected;
    }

    public void Complete()
    {
        if (Status != VacationStatus.Approved || DateTime.Today <= EndDate.Date)
        {
            throw new InvalidOperationException("Somente férias aprovadas e com período encerrado podem ser concluídas.");
        }

        Status = VacationStatus.Completed;
    }

    public int GetTotalDays()
    {
        if (StartDate == default || EndDate == default || EndDate.Date < StartDate.Date)
        {
            throw new InvalidOperationException("O período de férias é inválido.");
        }

        return (EndDate.Date - StartDate.Date).Days + 1;
    }

    private void EnsureNoOverlap(IEnumerable<Vacation> existingVacations)
    {
        ArgumentNullException.ThrowIfNull(existingVacations);
        if (existingVacations.Any(v => v.Id != Id && v.EmployeeId == EmployeeId &&
            v.Status != VacationStatus.Rejected &&
            StartDate.Date <= v.EndDate.Date && EndDate.Date >= v.StartDate.Date))
        {
            throw new InvalidOperationException("Já existe uma solicitação de férias nesse período para o colaborador.");
        }
    }
}

public enum VacationStatus 
{ 
    Requested, UnderReview, Approved, Rejected, Completed 
}
