/*Dominio Department*/
public class Department
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    public ICollection<Employee> Employees { get; set; }

    public void UpdateDetails(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);
        Name = name.Trim();
        Description = description.Trim();
    }

    public int CountEmployees()
    {
        return Employees?.Count ?? 0;
    }
}


