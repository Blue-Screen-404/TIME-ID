using TimeId.DTOs.Geolocation;
using TimeId.Operations;
namespace TimeId.Services;

public sealed record LocationValidationResult(string Status, Guid? WorkLocationId, double? DistanceMeters);

public sealed class GeolocationValidationService(DataStore store)
{
    public static void ValidateCoordinates(double? latitude, double? longitude)
    {
        if (latitude is not double lat || longitude is not double lon || !double.IsFinite(lat) || !double.IsFinite(lon)
            || lat < -90 || lat > 90 || lon < -180 || lon > 180)
            throw new DomainError(400, "Latitude ou longitude inválida.");
    }

    public static double DistanceMeters(double latitude, double longitude, double targetLatitude, double targetLongitude)
    {
        const double radians = Math.PI / 180;
        var dLat = (targetLatitude - latitude) * radians;
        var dLon = (targetLongitude - longitude) * radians;
        var a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(latitude * radians) * Math.Cos(targetLatitude * radians) * Math.Pow(Math.Sin(dLon / 2), 2);
        return 6371000 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }

    public LocationValidationResult Validate(Guid actorId, Guid employeeId, PunchLocationInput input) => store.Read(db =>
    {
        var actor = DataStore.Actor(db, actorId);
        if (actor.Role != "Administrador" && actor.EmployeeId != employeeId) throw new DomainError(403, "Você só pode validar sua própria localização.");
        var employee = db.Employees.FirstOrDefault(x => x.Id == employeeId && x.Active) ?? throw new DomainError(404, "Colaborador ativo não encontrado.");
        return Evaluate(db, employee, input);
    });

    // Usa a mesma cópia transacional do ponto; não lê um estado separado do armazenamento.
    public static LocationValidationResult Evaluate(Database db, Staff employee, PunchLocationInput input)
    {
        ValidateCoordinates(input.Latitude, input.Longitude);
        if (input.AccuracyMeters is not double accuracy || !double.IsFinite(accuracy) || accuracy < 0)
            throw new DomainError(400, "Informe uma precisão não negativa em metros.");
        var locations = db.WorkLocations.Where(x => WorkLocationService.Applies(x, employee))
            .Select(x => new { Location = x, Distance = DistanceMeters(input.Latitude!.Value, input.Longitude!.Value, x.Latitude, x.Longitude) }).ToList();
        if (locations.Count == 0) return new LocationValidationResult("SemRegra", null, null);
        var inside = locations.Where(x => x.Distance + accuracy <= x.Location.RadiusMeters).OrderBy(x => x.Distance).FirstOrDefault();
        if (inside != null) return new LocationValidationResult("DentroDaArea", inside.Location.Id, inside.Distance);
        var uncertain = locations.Where(x => x.Distance - accuracy <= x.Location.RadiusMeters).OrderBy(x => x.Distance).FirstOrDefault();
        if (uncertain != null) return new LocationValidationResult("Inconclusivo", uncertain.Location.Id, uncertain.Distance);
        var nearest = locations.OrderBy(x => x.Distance).First();
        return new LocationValidationResult("ForaDaArea", nearest.Location.Id, nearest.Distance);
    }
}
