/* Jornada semanal: os horários são locais e os dias indicam o início do turno. */
public class WorkSchedule
{
    public IReadOnlyCollection<DayOfWeek> WorkingDays { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }
    public TimeSpan BreakDuration { get; }

    public WorkSchedule(IEnumerable<DayOfWeek> workingDays, TimeOnly startTime,
        TimeOnly endTime, TimeSpan breakDuration)
    {
        ArgumentNullException.ThrowIfNull(workingDays);
        var days = workingDays.Distinct().ToArray();
        if (days.Length == 0 || days.Any(day => !Enum.IsDefined(day)))
        {
            throw new ArgumentException("Informe dias da semana válidos.", nameof(workingDays));
        }

        var duration = endTime - startTime;
        if (duration == TimeSpan.Zero || breakDuration < TimeSpan.Zero || breakDuration >= duration)
        {
            throw new ArgumentException("Informe horários diferentes e um intervalo menor que o turno.");
        }

        WorkingDays = Array.AsReadOnly(days);
        StartTime = startTime;
        EndTime = endTime;
        BreakDuration = breakDuration;
    }

    public bool IsWorkingDay(DateTime date)
    {
        return WorkingDays.Contains(date.DayOfWeek);
    }

    public TimeSpan GetExpectedWorkingTime(DateTime date)
    {
        return IsWorkingDay(date) ? (EndTime - StartTime) - BreakDuration : TimeSpan.Zero;
    }
}
