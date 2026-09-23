/*Dominio Role*/
public class Role 
{ 
    public Guid Id { get; set; } 
    public required string Name { get; set; }
    private readonly List<Permission> permissions = new();
    public IReadOnlyCollection<Permission> Permissions => permissions.AsReadOnly();

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void AddPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);
        if (permission.Id == Guid.Empty)
        {
            throw new ArgumentException("A permissão precisa ter um identificador.", nameof(permission));
        }

        if (!HasPermission(permission.Id))
        {
            permissions.Add(permission);
        }
    }

    public void RemovePermission(Guid permissionId)
    {
        permissions.RemoveAll(p => p.Id == permissionId);
    }

    public bool HasPermission(Guid permissionId)
    {
        return permissions.Any(p => p.Id == permissionId);
    }
}
