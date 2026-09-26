public class InMemoryEmployeeRepository
{
    private readonly List<Employee> employees = new();
    private readonly object sync = new();

    public List<Employee> GetAll()
    {
        lock (sync) return employees.ToList();
    }

    public Employee? GetById(Guid id)
    {
        lock (sync) return employees.FirstOrDefault(employee => employee.Id == id);
    }

    public void Add(Employee employee)
    {
        lock (sync) employees.Add(employee);
    }

    public void Remove(Guid id)
    {
        lock (sync) employees.RemoveAll(employee => employee.Id == id);
    }
}
