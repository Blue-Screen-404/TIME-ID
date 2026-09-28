public sealed class PositionService
{
    private readonly InMemoryPositionRepository repository;

    public PositionService(InMemoryPositionRepository repository) => this.repository = repository;

    public List<PositionOption> GetAll() => repository.GetAll()
        .Select(position => new PositionOption(position.Id, position.Name)).ToList();
}

public record PositionOption(Guid Id, string Name);
