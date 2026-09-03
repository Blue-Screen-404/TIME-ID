public class Holiday
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateTime Date { get; set; }

    public HolidayType Type { get; set; }
}

public enum HolidayType
{
    National,
    State,
    Municipal,
    Corporate
}