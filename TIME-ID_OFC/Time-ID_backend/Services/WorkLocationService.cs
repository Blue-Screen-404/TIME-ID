using TimeId.DTOs.Geolocation;
using TimeId.Operations;
namespace TimeId.Services;

public sealed class WorkLocationService(DataStore store)
{
    public static bool Applies(WorkLocation location, Staff employee) => location.Active &&
        (location.CompanyWide || location.DepartmentIds.Contains(employee.DepartmentId) || location.EmployeeIds.Contains(employee.Id));

    private static WorkLocationResponse Response(WorkLocation x) => new(x.Id, x.Name, x.Latitude, x.Longitude,
        x.RadiusMeters, x.CompanyWide, x.DepartmentIds.ToList(), x.EmployeeIds.ToList(), x.Active);

    public List<WorkLocationResponse> List(Guid actorId) => store.Read(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        GeolocationAccess.Require(actor.Role == "Administrador" || actor.Role == "Gestor" && db.GeolocationSettings.ManagersCanManageLocations);
        return db.WorkLocations.Where(x => GeolocationAccess.CanManage(db, actor, x)).OrderBy(x => x.Name).Select(Response).ToList();
    });

    public WorkLocationResponse Save(Guid actorId, Guid? id, CreateWorkLocationRequest input) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        GeolocationAccess.Require(actor.Role == "Administrador" || actor.Role == "Gestor" && db.GeolocationSettings.ManagersCanManageLocations);
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length < 2 || input.Name.Length > 120)
            throw new DomainError(400, "Informe um nome entre 2 e 120 caracteres.");
        GeolocationValidationService.ValidateCoordinates(input.Latitude, input.Longitude);
        if (input.RadiusMeters is not double radius || !double.IsFinite(radius) || radius <= 0 || radius > 100000)
            throw new DomainError(400, "Informe um raio maior que zero e de até 100.000 metros.");
        if (input.DepartmentIds is null || input.EmployeeIds is null)
            throw new DomainError(400, "Informe listas de permissões válidas.");
        var departments = input.DepartmentIds.Distinct().ToList();
        var employees = input.EmployeeIds.Distinct().ToList();
        if (!input.CompanyWide && departments.Count == 0 && employees.Count == 0)
            throw new DomainError(400, "Libere o local para a empresa, departamentos ou colaboradores.");
        if (departments.Any(id => !db.Departments.Any(x => x.Id == id)) || employees.Any(id => !db.Employees.Any(x => x.Id == id)))
            throw new DomainError(400, "Departamento ou colaborador não encontrado.");
        var location = id.HasValue ? db.WorkLocations.FirstOrDefault(x => x.Id == id.Value)
            ?? throw new DomainError(404, "Local não encontrado.") : new WorkLocation();
        if (id.HasValue) GeolocationAccess.Require(GeolocationAccess.CanManage(db, actor, location));
        GeolocationAccess.Require(GeolocationAccess.CanManage(db, actor, new WorkLocation { CompanyWide = input.CompanyWide, DepartmentIds = departments, EmployeeIds = employees }));
        location.Name = input.Name.Trim(); location.Latitude = input.Latitude!.Value; location.Longitude = input.Longitude!.Value;
        location.RadiusMeters = radius; location.CompanyWide = input.CompanyWide;
        location.DepartmentIds = departments; location.EmployeeIds = employees; location.Active = input.Active;
        if (!id.HasValue) db.WorkLocations.Add(location);
        DataStore.Log(db, actor, id.HasValue ? "Atualizou" : "Criou", "Local permitido", location.Id.ToString());
        return Response(location);
    });

    public WorkLocationResponse Deactivate(Guid actorId, Guid id) => store.Write(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        var location = db.WorkLocations.FirstOrDefault(x => x.Id == id) ?? throw new DomainError(404, "Local não encontrado.");
        GeolocationAccess.Require(GeolocationAccess.CanManage(db, actor, location));
        location.Active = false;
        DataStore.Log(db, actor, "Desativou", "Local permitido", id.ToString());
        return Response(location);
    });
}
