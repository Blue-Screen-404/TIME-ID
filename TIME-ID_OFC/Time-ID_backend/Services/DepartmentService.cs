using TimeId.Operations;

public sealed class DepartmentService(DataStore store)
{
    public List<DepartmentOption> GetAll() => store.Read(db => db.Departments
        .OrderBy(department => department.Name)
        .Select(department => new DepartmentOption(department.Id, department.Name)).ToList());

    public Team Save(Guid actorId, Team input, Guid? id) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        DataStore.Admin(actor);
        var team = id is null ? new Team() : db.Departments.FirstOrDefault(d => d.Id == id.Value)
            ?? throw new DomainError(404, "Departamento não encontrado.");
        Require(input.Name.Trim().Length >= 2, "Informe o nome do departamento.");
        Require(!db.Departments.Any(d => d.Id != team.Id && d.Name.Equals(input.Name.Trim(), StringComparison.OrdinalIgnoreCase)), "Departamento já cadastrado.");
        if (input.LeaderId is not null)
            Require(db.Employees.Any(e => e.Id == input.LeaderId && e.Active && e.DepartmentId == team.Id), "O líder deve ser um funcionário ativo deste departamento.");
        team.Name = input.Name.Trim(); team.Description = (input.Description ?? "").Trim(); team.LeaderId = input.LeaderId;
        if (id is null) db.Departments.Add(team);
        DataStore.Log(db, actor, id is null ? "Criou" : "Editou", "Departamento", team.Name);
        return team;
    });

    public bool Delete(Guid actorId, Guid id) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        DataStore.Admin(actor);
        var team = db.Departments.FirstOrDefault(d => d.Id == id)
            ?? throw new DomainError(404, "Departamento não encontrado.");
        Require(!db.Employees.Any(e => e.DepartmentId == id), "Transfira os funcionários antes de excluir o departamento.");
        db.Departments.Remove(team);
        DataStore.Log(db, actor, "Excluiu", "Departamento", team.Name);
        return true;
    });

    private static void Require(bool valid, string message) { if (!valid) throw new DomainError(400, message); }
}

public record DepartmentOption(Guid Id, string Name);
