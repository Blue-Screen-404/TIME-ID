public class PositionService
{
    private readonly InMemoryPositionRepository repository;
    private readonly InMemoryEmployeeRepository employees;

    public PositionService(InMemoryPositionRepository repository, InMemoryEmployeeRepository employees)
    { this.repository = repository; this.employees = employees; }

    public List<PositionResponse> GetAll() => repository.GetAll().OrderBy(p => p.Name)
        .Select(p => new PositionResponse(p.Id, p.Name, p.Description)).ToList();
    public PositionResponse? GetById(Guid id) => repository.GetById(id) is { } p ? new(p.Id, p.Name, p.Description) : null;

    public PositionResponse Create(PositionRequest request)
    {
        EnsureUniqueName(request.Name, null);
        var position = new Position { Id = Guid.NewGuid(), Name = request.Name.Trim(), Description = request.Description.Trim() };
        repository.Add(position);
        return new(position.Id, position.Name, position.Description);
    }

    public PositionResponse? Update(Guid id, PositionRequest request)
    {
        var position = repository.GetById(id);
        if (position is null) return null;
        EnsureUniqueName(request.Name, id);
        position.UpdateDetails(request.Name, request.Description);
        return new(position.Id, position.Name, position.Description);
    }

    public bool Delete(Guid id)
    {
        if (repository.GetById(id) is null) return false;
        if (employees.GetAll().Any(e => e.PositionId == id))
            throw new InvalidOperationException("Não é possível excluir um cargo associado a funcionários.");
        repository.Remove(id);
        return true;
    }

    private void EnsureUniqueName(string name, Guid? exceptId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Informe o nome do cargo.");
        if (repository.GetAll().Any(p => p.Id != exceptId && string.Equals(p.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Já existe um cargo com esse nome.");
    }
}
