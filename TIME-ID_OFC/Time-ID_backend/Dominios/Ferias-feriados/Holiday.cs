/*Dominio Holiday*/
public class Holiday
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateTime Date { get; set; }

    public HolidayType Type { get; set; }

    public void UpdateDetails(string name, DateTime date, HolidayType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (date == default || !Enum.IsDefined(type))
        {
            throw new ArgumentException("Informe data e tipo válidos.");
        }

        Name = name.Trim();
        Date = date.Date;
        Type = type;
    }

    public bool OccursOn(DateTime date)
    {
        return Date.Date == date.Date;
    }
}

public enum HolidayType
{
    National,
    State,
    Municipal,
    Corporate
}
