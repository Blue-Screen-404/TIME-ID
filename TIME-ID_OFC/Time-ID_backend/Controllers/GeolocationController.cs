using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeId.Operations;
using TimeId.Services;
namespace TimeId.Controllers;

[ApiController, Authorize, Route("api/geolocation")]
public sealed class GeolocationController(DataStore store, ReverseGeocodingService geocoder) : ControllerBase
{
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet("provider-status")]
    public IActionResult ProviderStatus() => Ok(store.Read(db =>
    {
        DataStore.Admin(DataStore.Actor(db, ActorId));
        return new { configured = geocoder.IsConfigured, enabled = db.GeolocationSettings.ReverseGeocodingEnabled };
    }));

    [HttpPost("punches/{id:guid}/retry-geocoding")]
    public IActionResult Retry(Guid id) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        var g = db.Punches.FirstOrDefault(p => p.Id == id)?.Geolocation ?? throw new DomainError(404, "Captura não encontrada.");
        if (g.PurgedAt != null || g.Latitude == null || g.Longitude == null) throw new DomainError(400, "Captura sem coordenadas disponíveis.");
        if (g.ApproximateLocation != null) throw new DomainError(400, "A captura já possui local aproximado.");
        if (!geocoder.IsConfigured || !db.GeolocationSettings.ReverseGeocodingEnabled) throw new DomainError(409, "Configure e habilite o provedor de geocodificação.");
        g.GeocodingAttempts = 0; g.NextGeocodingAttemptAt = null; g.GeocodingStatus = "Pendente";
        DataStore.Log(db, actor, "Solicitou geocodificação", "Ponto", id.ToString());
        return new { queued = true };
    }));
    [HttpGet("settings")]
    public IActionResult Settings() => Ok(store.Read(db => { DataStore.Admin(DataStore.Actor(db, ActorId)); return db.GeolocationSettings; }));
    [HttpPut("settings")]
    public IActionResult UpdateSettings(GeolocationSettings input) => Ok(store.Write(db =>
    {
        var actor = DataStore.Actor(db, ActorId); DataStore.Admin(actor);
        db.GeolocationSettings = input;
        DataStore.Log(db, actor, "Atualizou", "Configuração de geolocalização", "Política atualizada.");
        return input;
    }));
    [HttpGet("reviews")]
    public IActionResult Reviews([FromQuery] int page = 1) => Ok(store.Read(db =>
    {
        var actor = DataStore.Actor(db, ActorId);
        GeolocationAccess.Require(actor.Role == "Administrador" || actor.Role == "Gestor" && db.GeolocationSettings.ManagersCanReview);
        if (page < 1 || page > 100000) throw new DomainError(400, "Página inválida.");
        var pending = db.Punches.Where(p => p.Geolocation?.ReviewStatus == "Pendente" && GeolocationAccess.CanReview(db, actor, p.EmployeeId)).OrderBy(p => p.At).ToList();
        return new { total = pending.Count, page, pageSize = 50, items = pending.Skip((page - 1) * 50).Take(50).ToList() };
    }));
}
