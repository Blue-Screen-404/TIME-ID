/*Dominio Permission*/
public class Permission 
{ 
    public Guid Id { get; set; } 
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

    public void UpdateDetails(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);
        Name = name.Trim();
        Description = description.Trim();
    }
}
