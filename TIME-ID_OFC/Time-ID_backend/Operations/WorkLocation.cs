namespace TimeId.Operations;

public sealed class WorkLocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; }
    public bool CompanyWide { get; set; }
    public List<Guid> DepartmentIds { get; set; } = new();
    public List<Guid> EmployeeIds { get; set; } = new();
    public bool Active { get; set; } = true;
}
