using System.ComponentModel.DataAnnotations;

public class PositionRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
}

public record PositionResponse(Guid Id, string Name, string Description);
