using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace TimeId.DTOs.Geolocation;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ReviewPunchLocationRequest
{
    [Required, RegularExpression("^(Aceita|Recusada)$")] public string Decision { get; set; } = "";
    [Required, StringLength(500, MinimumLength = 5)] public string Reason { get; set; } = "";
}
