using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateProfileRequest
{
    [StringLength(100, MinimumLength = 2)]
    public string? Username { get; set; }
    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }
    [StringLength(2800000)]
    public string? PhotoUrl { get; set; }
}
