using TimeId.Operations;
namespace TimeId.Services;

public sealed class GeolocationMaintenanceService(DataStore store, ReverseGeocodingService geocoder)
{
    public int PurgeExpired(DateTimeOffset now)
    {
        var expired = store.Read(db => db.GeolocationSettings.RetentionDays is int days && db.Punches.Any(p => p.Geolocation is { PurgedAt: null } g && g.ReceivedAt < now.AddDays(-days)));
        if (!expired) return 0;
        return store.Write(db =>
        {
            if (db.GeolocationSettings.RetentionDays is not int days) return 0;
            int count = 0;
            foreach (var punch in db.Punches.Where(p => p.Geolocation is { PurgedAt: null } g && g.ReceivedAt < now.AddDays(-days)))
            {
                var g = punch.Geolocation!;
                g.Latitude = null; g.Longitude = null; g.AccuracyMeters = null; g.CapturedAt = null;
                g.WorkLocationId = null; g.WorkLocationName = null; g.WorkLocationLatitude = null; g.WorkLocationLongitude = null;
                g.RadiusMeters = null; g.DistanceMeters = null; g.ApproximateLocation = null; g.Attribution = null;
                g.Justification = null; g.ReviewReason = null; g.PurgedAt = now; g.GeocodingStatus = "Removida";
                if (g.ReviewStatus == "Pendente") g.ReviewStatus = "Expirada";
                for (int i = 0; i < db.Audit.Count; i++)
                    if (db.Audit[i].Action == "Analisou localização" && db.Audit[i].Detail.StartsWith(punch.Id.ToString(), StringComparison.Ordinal))
                        db.Audit[i] = db.Audit[i] with { Detail = $"{punch.Id}: dados de localização removidos por retenção." };
                count++;
            }
            return count;
        });
    }

    public async Task<bool> EnrichNextAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!geocoder.IsConfigured) return false;
        var next = store.Read(db => !db.GeolocationSettings.ReverseGeocodingEnabled ? null : db.Punches
            .Where(p => p.Geolocation is { PurgedAt: null, Latitude: not null, Longitude: not null, ApproximateLocation: null, GeocodingAttempts: < 3 } g &&
                (g.NextGeocodingAttemptAt == null || g.NextGeocodingAttemptAt <= now) &&
                (db.GeolocationSettings.RetentionDays is not int days || g.ReceivedAt >= now.AddDays(-days)))
            .OrderBy(p => p.At).FirstOrDefault());
        if (next == null) return false;
        // Reaproveita resolução das mesmas coordenadas ainda dentro da retenção, inclusive após reinício.
        ApproximatePlace? result = store.Read(db => db.Punches.Where(p => p.Geolocation is { PurgedAt: null, ApproximateLocation: not null } g &&
            g.Latitude == next.Geolocation!.Latitude && g.Longitude == next.Geolocation.Longitude &&
            g.ReceivedAt >= now.AddDays(-1) && (db.GeolocationSettings.RetentionDays is not int days || g.ReceivedAt >= now.AddDays(-days)))
            .Select(p => new ApproximatePlace(p.Geolocation!.ApproximateLocation!, p.Geolocation.Attribution ?? "")).FirstOrDefault());
        try { result ??= await geocoder.ResolveAsync(next.Geolocation!.Latitude!.Value, next.Geolocation.Longitude!.Value, cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException) { }
        store.Write(db =>
        {
            var g = db.Punches.FirstOrDefault(p => p.Id == next.Id)?.Geolocation;
            if (g == null || g.PurgedAt != null || !db.GeolocationSettings.ReverseGeocodingEnabled) return false;
            if (db.GeolocationSettings.RetentionDays is int days && g.ReceivedAt < now.AddDays(-days)) return false;
            g.GeocodingAttempts++; g.NextGeocodingAttemptAt = now.AddMinutes(5 * g.GeocodingAttempts);
            g.ApproximateLocation = result?.Name; g.Attribution = result?.Attribution;
            g.GeocodingStatus = result != null ? "Resolvida" : g.GeocodingAttempts >= 3 ? "Indisponivel" : "AguardandoNovaTentativa";
            return true;
        });
        return true;
    }
}

public sealed class GeolocationWorker(GeolocationMaintenanceService maintenance, ILogger<GeolocationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                maintenance.PurgeExpired(DateTimeOffset.UtcNow);
                await maintenance.EnrichNextAsync(DateTimeOffset.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception) { logger.LogWarning("Manutenção de geolocalização indisponível; nova tentativa no próximo ciclo."); }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
