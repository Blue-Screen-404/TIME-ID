using TimeId.Operations;

public sealed class PositionService(DataStore store)
{
    private static readonly string[] DefaultPositions = { "Analista", "Assistente", "Coordenador", "Gerente" };

    public List<PositionOption> GetAll() => store.Read(db => DefaultPositions
        .Concat(db.Employees.Select(employee => employee.Position))
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
        .Select(name => new PositionOption(EmployeeService.PositionId(name), name)).ToList());
}

public record PositionOption(Guid Id, string Name);
