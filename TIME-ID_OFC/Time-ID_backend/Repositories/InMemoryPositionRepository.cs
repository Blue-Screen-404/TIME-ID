public sealed class InMemoryPositionRepository
{
    private readonly List<Position> positions = new()
    {
        new() { Id = Guid.NewGuid(), Name = "Analista" },
        new() { Id = Guid.NewGuid(), Name = "Assistente" },
        new() { Id = Guid.NewGuid(), Name = "Coordenador" },
        new() { Id = Guid.NewGuid(), Name = "Gerente" }
    };

    public List<Position> GetAll() => positions.OrderBy(item => item.Name).ToList();
    public Position? GetById(Guid id) => positions.FirstOrDefault(item => item.Id == id);
}
