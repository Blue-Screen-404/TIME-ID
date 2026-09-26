public class EmployeeService
{
    private readonly InMemoryEmployeeRepository repository;
    private readonly InMemoryDepartmentRepository departments;
    private readonly InMemoryPositionRepository positions;

    public EmployeeService(InMemoryEmployeeRepository repository, InMemoryDepartmentRepository departments,
        InMemoryPositionRepository positions)
    { this.repository = repository; this.departments = departments; this.positions = positions; }

    public List<EmployeeResponse> GetAll(string? search = null, EmployeeStatus? status = null)
    {
        var query = repository.GetAll().AsEnumerable();
        if (status.HasValue) query = query.Where(e => e.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(e => e.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.RegistrationNumber.Contains(term, StringComparison.OrdinalIgnoreCase) || e.CPF.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        return query.OrderBy(e => e.FullName).Select(ToResponse).ToList();
    }

    public EmployeeResponse? GetById(Guid id) => repository.GetById(id) is { } employee ? ToResponse(employee) : null;

    public EmployeeResponse Create(CreateEmployeeRequest request)
    {
        ValidateCommon(request.RegistrationNumber, request.CPF, request.BirthDate, request.HireDate,
            request.DepartmentId, request.PositionId, request.LeaderId, null);
        var department = departments.GetById(request.DepartmentId)!;
        var position = positions.GetById(request.PositionId)!;
        var employee = new Employee
        {
            Id = Guid.NewGuid(), RegistrationNumber = request.RegistrationNumber.Trim(),
            FullName = request.FullName.Trim(), CPF = NormalizeCpf(request.CPF), Email = request.Email.Trim(),
            Phone = request.Phone.Trim(), BirthDate = request.BirthDate.Date, HireDate = request.HireDate.Date,
            DepartmentId = department.Id, Department = department,
            PositionId = position.Id, Position = position, Notes = request.Notes?.Trim()
        };
        employee.Activate();
        if (request.LeaderId is Guid leaderId) employee.AssignLeader(repository.GetById(leaderId)!);
        repository.Add(employee);
        department.Employees.Add(employee);
        return ToResponse(employee);
    }

    public EmployeeResponse? Update(Guid id, UpdateEmployeeRequest request)
    {
        var employee = repository.GetById(id);
        if (employee is null) return null;
        ValidateCommon(request.RegistrationNumber, request.CPF, request.BirthDate, request.HireDate,
            request.DepartmentId, request.PositionId, request.LeaderId, id);
        var department = departments.GetById(request.DepartmentId)!;
        var position = positions.GetById(request.PositionId)!;
        employee.RegistrationNumber = request.RegistrationNumber.Trim();
        employee.FullName = request.FullName.Trim();
        employee.CPF = NormalizeCpf(request.CPF);
        employee.UpdateContact(request.Email, request.Phone);
        employee.BirthDate = request.BirthDate.Date;
        employee.HireDate = request.HireDate.Date;
        employee.ChangeDepartment(department);
        employee.ChangePosition(position);
        employee.Notes = request.Notes?.Trim();
        employee.Leader = null;
        employee.LeaderId = null;
        if (request.LeaderId is Guid leaderId) employee.AssignLeader(repository.GetById(leaderId)!);
        return ToResponse(employee);
    }

    public bool Deactivate(Guid id)
    {
        var employee = repository.GetById(id);
        if (employee is null) return false;
        employee.Deactivate();
        return true;
    }

    private void ValidateCommon(string registration, string cpf, DateTime birthDate, DateTime hireDate,
        Guid departmentId, Guid positionId, Guid? leaderId, Guid? exceptId)
    {
        if (string.IsNullOrWhiteSpace(registration) || string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("Matrícula e CPF são obrigatórios.");
        if (birthDate == default || hireDate == default || birthDate.Date > DateTime.Today || hireDate.Date > DateTime.Today)
            throw new ArgumentException("Informe datas de nascimento e admissão válidas.");
        if (departments.GetById(departmentId) is null) throw new ArgumentException("Departamento não encontrado.");
        if (positions.GetById(positionId) is null) throw new ArgumentException("Cargo não encontrado.");
        string normalizedCpf = NormalizeCpf(cpf);
        var all = repository.GetAll();
        if (all.Any(e => e.Id != exceptId && string.Equals(e.RegistrationNumber, registration.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Já existe funcionário com essa matrícula.");
        if (all.Any(e => e.Id != exceptId && e.CPF == normalizedCpf))
            throw new InvalidOperationException("Já existe funcionário com esse CPF.");
        if (leaderId.HasValue && (leaderId == exceptId || repository.GetById(leaderId.Value) is null))
            throw new ArgumentException("Líder inválido.");
    }

    private static string NormalizeCpf(string cpf)
    {
        string digits = new(cpf.Where(char.IsDigit).ToArray());
        if (digits.Length != 11) throw new ArgumentException("O CPF deve conter 11 dígitos.");
        return digits;
    }

    private static EmployeeResponse ToResponse(Employee e) => new(e.Id, e.RegistrationNumber, e.FullName,
        e.CPF, e.Email, e.Phone, e.BirthDate, e.HireDate, e.DepartmentId, e.Department.Name,
        e.PositionId, e.Position.Name, e.LeaderId, e.Leader?.FullName, e.Status.ToString(), e.Notes);
}
