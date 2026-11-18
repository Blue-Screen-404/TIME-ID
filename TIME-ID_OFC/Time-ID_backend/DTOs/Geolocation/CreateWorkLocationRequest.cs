using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TimeId.DTOs.Geolocation;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CreateWorkLocationRequest
{
    [Required, StringLength(120, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required] public double? Latitude { get; set; }
    [Required] public double? Longitude { get; set; }
    [Required] public double? RadiusMeters { get; set; }
    public bool CompanyWide { get; set; }
    public List<Guid> DepartmentIds { get; set; } = new();
    public List<Guid> EmployeeIds { get; set; } = new();
    public bool Active { get; set; } = true;
}
