namespace TimeId.Operations;

// Snapshot da captura e da regra: não é recalculado quando um local é editado.
public sealed class PunchGeolocation
{
    public string? ApproximateLocation { get; set; }
    public string? Attribution { get; set; }
    public string GeocodingStatus { get; set; } = "Pendente";
    public int GeocodingAttempts { get; set; }
    public DateTimeOffset? NextGeocodingAttemptAt { get; set; }
    public DateTimeOffset? PurgedAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? AccuracyMeters { get; set; }
    public DateTimeOffset? CapturedAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public string Status { get; set; } = "SemRegra";
    public Guid? WorkLocationId { get; set; }
    public string? WorkLocationName { get; set; }
    public double? WorkLocationLatitude { get; set; }
    public double? WorkLocationLongitude { get; set; }
    public double? RadiusMeters { get; set; }
    public double? DistanceMeters { get; set; }
    public string? Justification { get; set; }
    public string ReviewStatus { get; set; } = "NaoNecessaria";
    public Guid? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewReason { get; set; }
}
