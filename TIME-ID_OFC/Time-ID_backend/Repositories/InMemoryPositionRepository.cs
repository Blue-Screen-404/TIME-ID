public class InMemoryPositionRepository
{
    private readonly List<Position> positions = new();
    private readonly object sync = new();

    public List<Position> GetAll()
    {
        lock (sync) return positions.ToList();
    }

    public Position? GetById(Guid id)
    {
        lock (sync) return positions.FirstOrDefault(position => position.Id == id);
    }

    public void Add(Position position)
    {
        lock (sync) positions.Add(position);
    }

    public void Remove(Guid id)
    {
        lock (sync) positions.RemoveAll(position => position.Id == id);
    }
}
