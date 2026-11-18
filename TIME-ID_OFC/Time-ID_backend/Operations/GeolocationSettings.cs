using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace TimeId.Operations;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class GeolocationSettings
{
    [Range(1, 3600)] public int MaxCaptureAgeSeconds { get; set; } = 300;
    [Range(0, 300)] public int FutureToleranceSeconds { get; set; } = 30;
    public bool RequireJustification { get; set; } = true;
    public bool ManagersCanManageLocations { get; set; }
    public bool ManagersCanReview { get; set; }
    [Range(1, 36500)] public int? RetentionDays { get; set; }
    public bool ReverseGeocodingEnabled { get; set; }
}
