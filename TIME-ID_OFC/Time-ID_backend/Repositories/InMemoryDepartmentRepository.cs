public class InMemoryDepartmentRepository
{
    private readonly List<Department> departments = new();
    private readonly object sync = new();

    public List<Department> GetAll()
    {
        lock (sync) return departments.ToList();
    }

    public Department? GetById(Guid id)
    {
        lock (sync) return departments.FirstOrDefault(department => department.Id == id);
    }

    public void Add(Department department)
    {
        lock (sync) departments.Add(department);
    }

    public void Remove(Guid id)
    {
        lock (sync) departments.RemoveAll(department => department.Id == id);
    }
}
