public sealed class InMemoryEmployeeRepository
{
    private readonly List<Employee> employees = new();
    private readonly object gate = new();

    public List<Employee> GetAll()
    {
        lock (gate) return employees.ToList();
    }

    public Employee? GetById(Guid id)
    {
        lock (gate) return employees.FirstOrDefault(employee => employee.Id == id);
    }

    public void Add(Employee employee)
    {
        lock (gate) employees.Add(employee);
    }
}
