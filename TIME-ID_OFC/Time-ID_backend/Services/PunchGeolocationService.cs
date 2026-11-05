using TimeId.Operations;
namespace TimeId.Services;

public static class PunchGeolocationService
{
    public static PunchGeolocation Capture(Database db, Staff staff, PunchInput input, DateTimeOffset now)
    {
        var result = new PunchGeolocation { ReceivedAt = now };
        bool hasRules = db.WorkLocations.Any(x => WorkLocationService.Applies(x, staff));
        if (input.Location is { } position)
        {
            var evaluation = GeolocationValidationService.Evaluate(db, staff, position);
            if (position.CapturedAt is not DateTimeOffset captured)
                throw new DomainError(400, "Informe o horário da captura da localização.");
            result.Latitude = position.Latitude; result.Longitude = position.Longitude;
            result.AccuracyMeters = position.AccuracyMeters; result.CapturedAt = captured;
            result.Status = evaluation.Status; result.DistanceMeters = evaluation.DistanceMeters;
            result.WorkLocationId = evaluation.WorkLocationId;
            if (evaluation.WorkLocationId is Guid id)
            {
                var location = db.WorkLocations.Single(x => x.Id == id);
                result.WorkLocationName = location.Name;
                result.WorkLocationLatitude = location.Latitude; result.WorkLocationLongitude = location.Longitude;
                result.RadiusMeters = location.RadiusMeters;
            }
            // Horários e posições declarados pelo cliente não constituem prova de autenticidade.
            if (hasRules && (captured < now.AddSeconds(-db.GeolocationSettings.MaxCaptureAgeSeconds) || captured > now.AddSeconds(db.GeolocationSettings.FutureToleranceSeconds)))
                result.Status = "CapturaDesatualizada";
        }
        else result.Status = hasRules ? "SemLocalizacao" : "SemRegra";
        var reason = input.LocationJustification?.Trim();
        if (reason?.Length > 500) throw new DomainError(400, "A justificativa deve ter até 500 caracteres.");
        bool needsReview = hasRules && result.Status != "DentroDaArea";
        if (needsReview && db.GeolocationSettings.RequireJustification && (reason is null || reason.Length < 5))
            throw new DomainError(400, "Informe uma justificativa de 5 a 500 caracteres para a localização não validada.");
        result.Justification = string.IsNullOrEmpty(reason) ? null : reason;
        result.ReviewStatus = needsReview ? "Pendente" : "NaoNecessaria";
        return result;
    }
}
