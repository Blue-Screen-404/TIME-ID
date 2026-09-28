public sealed class InMemoryDepartmentRepository
{
    private readonly List<Department> departments = new()
    {
        new() { Id = Guid.NewGuid(), Name = "Administrativo" },
        new() { Id = Guid.NewGuid(), Name = "Financeiro" },
        new() { Id = Guid.NewGuid(), Name = "Recursos Humanos" },
        new() { Id = Guid.NewGuid(), Name = "Tecnologia" }
    };

    public List<Department> GetAll() => departments.OrderBy(item => item.Name).ToList();
    public Department? GetById(Guid id) => departments.FirstOrDefault(item => item.Id == id);
}
