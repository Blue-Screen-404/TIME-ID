using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TimeId.Operations;

public sealed class EmployeeService(DataStore store)
{
    private static readonly string[] ValidWorkDays =
        { "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado", "Domingo" };
    private static readonly string[] DefaultPositions = { "Analista", "Assistente", "Coordenador", "Gerente" };
    private static readonly string[] AllStates = "AC AL AP AM BA CE DF ES GO MA MT MS MG PA PB PR PE PI RJ RN RS RO RR SC SP SE TO".Split(' ');

    public Staff Save(Guid actorId, StaffInput input, Guid? id) => store.Write(db => Save(db, actorId, input, id, null, updateLeader: false));

    public bool Delete(Guid actorId, Guid id) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        DataStore.Admin(actor);
        var employee = db.Employees.FirstOrDefault(e => e.Id == id) ?? throw new DomainError(404, "Funcionário não encontrado.");
        Require(!db.Punches.Any(p => p.EmployeeId == id) && !db.Vacations.Any(v => v.EmployeeId == id), "Há histórico de ponto ou férias. Inative o funcionário para preservar os registros.");
        Require(!db.Accounts.Any(a => a.Id == actor.Id && a.EmployeeId == id), "Você não pode excluir sua própria conta.");
        foreach (var team in db.Departments.Where(d => d.LeaderId == id)) team.LeaderId = null;
        foreach (var subordinate in db.Employees.Where(e => e.LeaderId == id)) subordinate.LeaderId = null;
        db.FaceProfiles.RemoveAll(profile => profile.EmployeeId == id);
        foreach (var account in db.Accounts.Where(a => a.EmployeeId == id)) { account.Active = false; account.EmployeeId = null; }
        db.Employees.Remove(employee);
        DataStore.Log(db, actor, "Excluiu", "Funcionário", employee.Name);
        return true;
    });

    public List<EmployeeResponse> GetAll(Guid actorId, string? search, EmployeeStatus? status) => store.Read(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        IEnumerable<Staff> query = db.Employees.Where(employee => actor.Role == "Administrador" || employee.Id == actor.EmployeeId);
        if (status.HasValue) query = query.Where(employee => (employee.Active ? EmployeeStatus.Active : EmployeeStatus.Inactive) == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(employee => employee.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                employee.Registration.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                employee.Cpf.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        return query.OrderBy(employee => employee.Name).Select(employee => ToResponse(db, employee)).ToList();
    });

    public EmployeeResponse? GetById(Guid actorId, Guid id) => store.Read(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        var employee = db.Employees.FirstOrDefault(e => e.Id == id && (actor.Role == "Administrador" || e.Id == actor.EmployeeId));
        return employee is null ? null : ToResponse(db, employee);
    });

    public EmployeeResponse Create(Guid actorId, CreateEmployeeRequest request) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        DataStore.Admin(actor);
        string position = ResolvePosition(db, request.PositionId);
        var input = new StaffInput
        {
            Name = request.FullName, Registration = request.RegistrationNumber, Cpf = request.CPF,
            Email = request.Email, Phone = request.Phone ?? "", BirthDate = DateOnly.FromDateTime(request.BirthDate),
            HireDate = DateOnly.FromDateTime(request.HireDate), DepartmentId = request.DepartmentId,
            Position = position, WorkMode = "Presencial", WeeklyHours = 40, Active = true,
            AccessRole = "Usuário"
        };
        Staff employee = Save(db, actorId, input, null, request.LeaderId, updateLeader: true);
        return ToResponse(db, employee);
    });

    public EmployeeResponse? Update(Guid actorId, Guid id, UpdateEmployeeRequest request) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        DataStore.Admin(actor);
        Staff? current = db.Employees.FirstOrDefault(e => e.Id == id);
        if (current is null) return null;
        string position = ResolvePosition(db, request.PositionId);
        var input = ToInput(current);
        input.Name = request.FullName; input.Registration = request.RegistrationNumber;
        input.Cpf = request.CPF; input.Email = request.Email; input.Phone = request.Phone ?? "";
        input.BirthDate = DateOnly.FromDateTime(request.BirthDate); input.HireDate = DateOnly.FromDateTime(request.HireDate);
        input.DepartmentId = request.DepartmentId; input.Position = position;
        Staff employee = Save(db, actorId, input, id, request.LeaderId, updateLeader: true);
        return ToResponse(db, employee);
    });

    public bool Deactivate(Guid actorId, Guid id) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        DataStore.Admin(actor);
        Staff? employee = db.Employees.FirstOrDefault(e => e.Id == id);
        if (employee is null) return false;
        if (!employee.Active) return true;
        var input = ToInput(employee);
        input.Active = false;
        Save(db, actorId, input, id, null, updateLeader: false);
        return true;
    });

    public static Guid PositionId(string name)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes("TIMEID:position:" + name.Trim().ToUpperInvariant()));
        return new Guid(hash.AsSpan(0, 16));
    }

    private Staff Save(Database db, Guid actorId, StaffInput input, Guid? id, Guid? leaderId, bool updateLeader)
    {
        var actor = DataStore.Actor(db, actorId);
        DataStore.Admin(actor);
        var employee = id is null ? new Staff() : db.Employees.FirstOrDefault(e => e.Id == id.Value) ?? throw new DomainError(404, "Funcionário não encontrado.");
        string name = (input.Name ?? "").Trim();
        string registration = (input.Registration ?? "").Trim();
        string email = (input.Email ?? "").Trim();
        string positionName = (input.Position ?? "").Trim();
        string cpf = new((input.Cpf ?? "").Where(char.IsAsciiDigit).ToArray());
        Require(ValidCpf(cpf), "Informe um CPF válido.");
        Require(name.Length >= 2 && positionName.Length > 0 && registration.Length > 0, "Preencha nome, matrícula e cargo.");
        Require(input.BirthDate > new DateOnly(1900, 1, 1) && input.BirthDate < DataStore.Today, "Confira a data de nascimento.");
        Require(input.HireDate >= input.BirthDate && input.HireDate <= DataStore.Today.AddYears(1), "Confira a data de admissão.");
        Require(db.Departments.Any(d => d.Id == input.DepartmentId), "Selecione um departamento cadastrado.");
        Require(!db.Employees.Any(e => e.Id != employee.Id && (e.Cpf == cpf || e.Registration.Equals(registration, StringComparison.OrdinalIgnoreCase) || e.Email.Equals(email, StringComparison.OrdinalIgnoreCase))), "CPF, matrícula ou e-mail já cadastrado.");
        var linked = db.Accounts.FirstOrDefault(a => a.EmployeeId == employee.Id);
        var matching = db.Accounts.FirstOrDefault(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        Require(matching is null || matching == linked || matching.EmployeeId is null && linked is null, "E-mail já usado por outra conta.");
        if (linked is null && matching is not null) linked = matching;
        if (linked is not null && linked.Id == actor.Id) Require(input.Active, "Você não pode desativar sua própria conta.");
        if (linked is null && input.CreateAccess)
        {
            Require(!string.IsNullOrWhiteSpace(input.InitialPassword) && input.InitialPassword.Length >= 8, "Informe uma senha inicial com pelo menos 8 caracteres.");
            linked = new Account { Role = input.AccessRole };
            linked.PasswordHash = new PasswordHasher<Account>().HashPassword(linked, input.InitialPassword!);
            db.Accounts.Add(linked);
        }
        Require(input.AccessRole is "Administrador" or "Gestor" or "Usuário", "Perfil de acesso inválido.");
        foreach (var departmentLeader in db.Departments.Where(d => d.LeaderId == employee.Id && (d.Id != input.DepartmentId || !input.Active))) departmentLeader.LeaderId = null;

        var targetTeam = db.Departments.First(d => d.Id == input.DepartmentId);
        Employee domain;
        if (id is null)
        {
            domain = new Employee
            {
                Id = employee.Id, RegistrationNumber = registration, FullName = name, CPF = cpf,
                Email = email, Phone = (input.Phone ?? "").Trim(), BirthDate = input.BirthDate.ToDateTime(TimeOnly.MinValue),
                HireDate = input.HireDate.ToDateTime(TimeOnly.MinValue), DepartmentId = targetTeam.Id,
                Department = ToDomainDepartment(targetTeam), PositionId = PositionId(positionName),
                Position = new Position { Id = PositionId(positionName), Name = positionName }
            };
        }
        else domain = ToDomain(employee, db);
        domain.RegistrationNumber = registration; domain.FullName = name; domain.CPF = cpf;
        domain.UpdateContact(email, input.Phone ?? "");
        domain.BirthDate = input.BirthDate.ToDateTime(TimeOnly.MinValue); domain.HireDate = input.HireDate.ToDateTime(TimeOnly.MinValue);
        domain.ChangeDepartment(ToDomainDepartment(targetTeam));
        var position = new Position { Id = PositionId(positionName), Name = positionName };
        domain.ChangePosition(position);
        if (updateLeader)
        {
            domain.Leader = null; domain.LeaderId = null;
            if (leaderId is Guid requestedLeader)
            {
                var leader = db.Employees.FirstOrDefault(e => e.Id == requestedLeader && e.Active);
                Require(leader is not null && leader.Id != employee.Id, "O gestor selecionado é inválido.");
                Require(!WouldCreateLeadershipCycle(db, employee.Id, requestedLeader), "A liderança não pode criar um ciclo.");
                domain.AssignLeader(ToDomain(leader!, db));
            }
        }
        else if (employee.LeaderId is Guid currentLeader)
        {
            var leader = db.Employees.FirstOrDefault(e => e.Id == currentLeader);
            if (leader is not null) domain.AssignLeader(ToDomain(leader, db));
        }
        if (input.Active) domain.Activate(); else domain.Deactivate();

        employee.Name = domain.FullName; employee.Cpf = domain.CPF; employee.Registration = domain.RegistrationNumber;
        employee.Email = domain.Email; employee.Phone = domain.Phone; employee.BirthDate = input.BirthDate;
        employee.HireDate = input.HireDate; employee.DepartmentId = domain.DepartmentId; employee.Position = domain.Position.Name;
        employee.LeaderId = domain.LeaderId;
        Require(input.WorkMode is "Presencial" or "Híbrido" or "Remoto", "Selecione um regime de trabalho válido.");
        var requestedWorkDays = input.WorkDays ?? new List<string>();
        Require(requestedWorkDays.Distinct().Count() == requestedWorkDays.Count && requestedWorkDays.All(ValidWorkDays.Contains), "Selecione dias presenciais válidos.");
        Require(input.WorkMode != "Híbrido" || requestedWorkDays.Count > 0, "Selecione pelo menos um dia presencial para o regime híbrido.");
        employee.WorkMode = input.WorkMode;
        employee.WorkDays = input.WorkMode == "Híbrido" ? requestedWorkDays.ToList() : new List<string>();
        if (!input.Active || input.WorkMode == "Remoto") db.FaceProfiles.RemoveAll(profile => profile.EmployeeId == employee.Id);
        string postalCode = new((input.PostalCode ?? "").Where(char.IsDigit).ToArray());
        Require(postalCode.Length == 0 || (postalCode.Length == 8 && System.Text.RegularExpressions.Regex.IsMatch(input.PostalCode!, @"^\d{5}-?\d{3}$")), "Informe um CEP com 8 dígitos.");
        string state = (input.State ?? "").Trim().ToUpperInvariant();
        Require(state.Length == 0 || AllStates.Contains(state), "Selecione uma UF válida.");
        employee.PostalCode = postalCode; employee.Street = (input.Street ?? "").Trim(); employee.Number = (input.Number ?? "").Trim();
        employee.Complement = (input.Complement ?? "").Trim(); employee.Neighborhood = (input.Neighborhood ?? "").Trim();
        employee.City = (input.City ?? "").Trim(); employee.State = state; employee.Duties = (input.Duties ?? "").Trim();
        employee.WeeklyHours = input.WeeklyHours; employee.Address = (input.Address ?? "").Trim(); employee.Rg = (input.Rg ?? "").Trim();
        employee.Active = input.Active;
        if (!input.Active)
            foreach (var subordinate in db.Employees.Where(e => e.Id != employee.Id && e.LeaderId == employee.Id)) subordinate.LeaderId = null;
        if (id is null) db.Employees.Add(employee);
        if (linked is not null) { linked.EmployeeId = employee.Id; linked.Username = employee.Name; linked.Email = employee.Email; linked.Active = employee.Active; }
        DataStore.Log(db, actor, id is null ? "Criou" : "Editou", "Funcionário", employee.Name);
        return employee;
    }

    private static Employee ToDomain(Staff employee, Database db)
    {
        var team = db.Departments.FirstOrDefault(d => d.Id == employee.DepartmentId);
        var department = team is null ? new Department { Id = employee.DepartmentId, Name = "—" } : ToDomainDepartment(team);
        var position = new Position { Id = PositionId(employee.Position), Name = employee.Position };
        var domain = new Employee
        {
            Id = employee.Id, RegistrationNumber = employee.Registration, FullName = employee.Name, CPF = employee.Cpf,
            Email = employee.Email, Phone = employee.Phone, BirthDate = employee.BirthDate.ToDateTime(TimeOnly.MinValue),
            HireDate = employee.HireDate.ToDateTime(TimeOnly.MinValue), DepartmentId = department.Id, Department = department,
            PositionId = position.Id, Position = position
        };
        if (employee.Active) domain.Activate(); else domain.Deactivate();
        if (employee.LeaderId is Guid leaderId && db.Employees.FirstOrDefault(e => e.Id == leaderId) is { } leader && leader.Id != employee.Id)
            domain.AssignLeader(new Employee
            {
                Id = leader.Id, RegistrationNumber = leader.Registration, FullName = leader.Name, CPF = leader.Cpf,
                Email = leader.Email, Phone = leader.Phone, BirthDate = leader.BirthDate.ToDateTime(TimeOnly.MinValue),
                HireDate = leader.HireDate.ToDateTime(TimeOnly.MinValue), DepartmentId = leader.DepartmentId,
                Department = new Department { Id = leader.DepartmentId, Name = db.Departments.FirstOrDefault(d => d.Id == leader.DepartmentId)?.Name ?? "—" },
                PositionId = PositionId(leader.Position), Position = new Position { Id = PositionId(leader.Position), Name = leader.Position }
            });
        return domain;
    }

    private static Department ToDomainDepartment(Team team) => new() { Id = team.Id, Name = team.Name };

    private static StaffInput ToInput(Staff employee) => new()
    {
        Name = employee.Name, Registration = employee.Registration, Cpf = employee.Cpf, Email = employee.Email,
        Phone = employee.Phone, BirthDate = employee.BirthDate, HireDate = employee.HireDate,
        DepartmentId = employee.DepartmentId, Position = employee.Position, WorkMode = employee.WorkMode,
        WorkDays = employee.WorkDays.ToList(), WeeklyHours = employee.WeeklyHours, Address = employee.Address,
        PostalCode = employee.PostalCode, Street = employee.Street, Number = employee.Number, Complement = employee.Complement,
        Neighborhood = employee.Neighborhood, City = employee.City, State = employee.State, Duties = employee.Duties,
        Rg = employee.Rg, Active = employee.Active, AccessRole = "Usuário"
    };

    private static EmployeeResponse ToResponse(Database db, Staff employee) => new(
        employee.Id, employee.Registration, employee.Name, employee.Cpf, employee.Email, employee.Phone,
        employee.BirthDate.ToDateTime(TimeOnly.MinValue), employee.HireDate.ToDateTime(TimeOnly.MinValue),
        employee.DepartmentId, db.Departments.FirstOrDefault(d => d.Id == employee.DepartmentId)?.Name ?? "—",
        PositionId(employee.Position), employee.Position, employee.LeaderId,
        employee.LeaderId is Guid leaderId ? db.Employees.FirstOrDefault(e => e.Id == leaderId)?.Name : null,
        employee.Active ? EmployeeStatus.Active.ToString() : EmployeeStatus.Inactive.ToString());

    private static string ResolvePosition(Database db, Guid id) => DefaultPositions
        .Concat(db.Employees.Select(e => e.Position)).FirstOrDefault(name => PositionId(name) == id)
        ?? throw new DomainError(400, "Cargo selecionado não encontrado.");

    private static bool WouldCreateLeadershipCycle(Database db, Guid employeeId, Guid proposedLeaderId)
    {
        var visited = new HashSet<Guid>();
        Guid? current = proposedLeaderId;
        while (current is Guid leaderId)
        {
            if (leaderId == employeeId || !visited.Add(leaderId)) return true;
            current = db.Employees.FirstOrDefault(employee => employee.Id == leaderId)?.LeaderId;
        }
        return false;
    }

    private static bool ValidCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;
        for (int length = 9; length <= 10; length++)
        {
            int sum = 0;
            for (int i = 0; i < length; i++) sum += (cpf[i] - '0') * (length + 1 - i);
            int digit = sum * 10 % 11; if (digit == 10) digit = 0;
            if (digit != cpf[length] - '0') return false;
        }
        return true;
    }

    private static void Require(bool valid, string message) { if (!valid) throw new DomainError(400, message); }
}
