public sealed class EmployeeService
{
    private readonly InMemoryEmployeeRepository employees;
    private readonly InMemoryDepartmentRepository departments;
    private readonly InMemoryPositionRepository positions;

    public EmployeeService(InMemoryEmployeeRepository employees,
        InMemoryDepartmentRepository departments, InMemoryPositionRepository positions)
    {
        this.employees = employees;
        this.departments = departments;
        this.positions = positions;
    }

    public List<EmployeeResponse> GetAll(string? search, EmployeeStatus? status)
    {
        IEnumerable<Employee> query = employees.GetAll();
        if (status.HasValue) query = query.Where(employee => employee.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(employee =>
                employee.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                employee.RegistrationNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                employee.CPF.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        return query.OrderBy(employee => employee.FullName).Select(ToResponse).ToList();
    }

    public EmployeeResponse? GetById(Guid id) =>
        employees.GetById(id) is { } employee ? ToResponse(employee) : null;

    public EmployeeResponse Create(CreateEmployeeRequest request)
    {
        Validate(request, null);
        Department department = departments.GetById(request.DepartmentId)!;
        Position position = positions.GetById(request.PositionId)!;
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            FullName = request.FullName.Trim(),
            CPF = NormalizeCpf(request.CPF),
            Email = request.Email.Trim(),
            Phone = request.Phone?.Trim() ?? string.Empty,
            BirthDate = request.BirthDate.Date,
            HireDate = request.HireDate.Date,
            DepartmentId = department.Id,
            Department = department,
            PositionId = position.Id,
            Position = position
        };
        employee.Activate();
        if (request.LeaderId is Guid leaderId)
            employee.AssignLeader(employees.GetById(leaderId)!);

        department.Employees.Add(employee);
        employees.Add(employee);
        return ToResponse(employee);
    }

    public EmployeeResponse? Update(Guid id, UpdateEmployeeRequest request)
    {
        Employee? employee = employees.GetById(id);
        if (employee is null) return null;
        Validate(request, id);

        Department department = departments.GetById(request.DepartmentId)!;
        Position position = positions.GetById(request.PositionId)!;
        employee.RegistrationNumber = request.RegistrationNumber.Trim();
        employee.FullName = request.FullName.Trim();
        employee.CPF = NormalizeCpf(request.CPF);
        employee.UpdateContact(request.Email, request.Phone);
        employee.BirthDate = request.BirthDate.Date;
        employee.HireDate = request.HireDate.Date;
        employee.ChangeDepartment(department);
        employee.ChangePosition(position);
        employee.Leader = null;
        employee.LeaderId = null;
        if (request.LeaderId is Guid leaderId)
            employee.AssignLeader(employees.GetById(leaderId)!);

        return ToResponse(employee);
    }

    public bool Deactivate(Guid id)
    {
        Employee? employee = employees.GetById(id);
        if (employee is null) return false;
        employee.Deactivate();
        return true;
    }

    private void Validate(CreateEmployeeRequest request, Guid? exceptId)
    {
        if (string.IsNullOrWhiteSpace(request.RegistrationNumber) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Matrícula, nome e e-mail são obrigatórios.");

        if (request.BirthDate == default || request.BirthDate.Date > DateTime.Today ||
            request.HireDate == default || request.HireDate.Date > DateTime.Today)
            throw new ArgumentException("Informe datas de nascimento e admissão válidas.");
        if (request.HireDate.Date < request.BirthDate.Date)
            throw new ArgumentException("A admissão não pode ser anterior ao nascimento.");
        if (departments.GetById(request.DepartmentId) is null)
            throw new ArgumentException("Departamento não encontrado.");
        if (positions.GetById(request.PositionId) is null)
            throw new ArgumentException("Cargo não encontrado.");

        string cpf = NormalizeCpf(request.CPF);
        List<Employee> all = employees.GetAll();
        if (all.Any(employee => employee.Id != exceptId && string.Equals(
                employee.RegistrationNumber, request.RegistrationNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Já existe um funcionário com essa matrícula.");
        if (all.Any(employee => employee.Id != exceptId && employee.CPF == cpf))
            throw new InvalidOperationException("Já existe um funcionário com esse CPF.");
        if (request.LeaderId.HasValue &&
            (request.LeaderId == exceptId || employees.GetById(request.LeaderId.Value) is null))
            throw new ArgumentException("O gestor selecionado é inválido.");

        if (!System.Net.Mail.MailAddress.TryCreate(request.Email.Trim(), out var address) ||
            address.Address != request.Email.Trim())
            throw new ArgumentException("Informe um e-mail válido.");
    }

    private static string NormalizeCpf(string cpf)
    {
        string digits = new(cpf.Where(char.IsDigit).ToArray());
        if (digits.Length != 11) throw new ArgumentException("O CPF deve conter 11 dígitos.");
        return digits;
    }

    private static EmployeeResponse ToResponse(Employee employee) => new(
        employee.Id,
        employee.RegistrationNumber,
        employee.FullName,
        employee.CPF,
        employee.Email,
        employee.Phone,
        employee.BirthDate,
        employee.HireDate,
        employee.DepartmentId,
        employee.Department.Name,
        employee.PositionId,
        employee.Position.Name,
        employee.LeaderId,
        employee.Leader?.FullName,
        employee.Status.ToString());
}
