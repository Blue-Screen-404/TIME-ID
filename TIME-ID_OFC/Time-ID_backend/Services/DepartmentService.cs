public class DepartmentService
{
    private readonly InMemoryDepartmentRepository repository;
    private readonly InMemoryEmployeeRepository employees;

    public DepartmentService(InMemoryDepartmentRepository repository, InMemoryEmployeeRepository employees)
    { this.repository = repository; this.employees = employees; }

    public List<DepartmentResponse> GetAll() => repository.GetAll()
        .OrderBy(d => d.Name).Select(ToResponse).ToList();
    public DepartmentResponse? GetById(Guid id) => repository.GetById(id) is { } item ? ToResponse(item) : null;

    public DepartmentResponse Create(DepartmentRequest request)
    {
        EnsureUniqueName(request.Name, null);
        var department = new Department { Id = Guid.NewGuid(), Name = request.Name.Trim(), Description = request.Description.Trim() };
        repository.Add(department);
        return ToResponse(department);
    }

    public DepartmentResponse? Update(Guid id, DepartmentRequest request)
    {
        var item = repository.GetById(id);
        if (item is null) return null;
        EnsureUniqueName(request.Name, id);
        item.UpdateDetails(request.Name, request.Description);
        return ToResponse(item);
    }

    public bool Delete(Guid id)
    {
        var item = repository.GetById(id);
        if (item is null) return false;
        if (employees.GetAll().Any(e => e.DepartmentId == id))
            throw new InvalidOperationException("Não é possível excluir um departamento que possui funcionários.");
        repository.Remove(id);
        return true;
    }

    private void EnsureUniqueName(string name, Guid? exceptId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Informe o nome do departamento.");
        if (repository.GetAll().Any(d => d.Id != exceptId && string.Equals(d.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Já existe um departamento com esse nome.");
    }

    private DepartmentResponse ToResponse(Department d) => new(d.Id, d.Name, d.Description,
        employees.GetAll().Count(e => e.DepartmentId == d.Id));
}
