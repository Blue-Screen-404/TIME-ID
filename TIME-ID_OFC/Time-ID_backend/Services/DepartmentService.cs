public sealed class DepartmentService
{
    private readonly InMemoryDepartmentRepository repository;

    public DepartmentService(InMemoryDepartmentRepository repository) => this.repository = repository;

    public List<DepartmentOption> GetAll() => repository.GetAll()
        .Select(department => new DepartmentOption(department.Id, department.Name)).ToList();
}

public record DepartmentOption(Guid Id, string Name);
