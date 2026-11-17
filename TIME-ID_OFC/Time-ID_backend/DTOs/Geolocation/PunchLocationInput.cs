using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace TimeId.DTOs.Geolocation;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PunchLocationInput
{
    public DateTimeOffset? CapturedAt { get; set; }
    [Required] public double? Latitude { get; set; }
    [Required] public double? Longitude { get; set; }
    [Required] public double? AccuracyMeters { get; set; }
}
