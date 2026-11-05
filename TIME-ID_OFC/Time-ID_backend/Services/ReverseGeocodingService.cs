using System.Globalization;
using System.Text.Json;
namespace TimeId.Services;

public sealed record ApproximatePlace(string Name, string Attribution);

// Endpoint exclusivo de um provedor Nominatim próprio/contratado, configurado na implantação.
// O servidor público da OSMF não é usado para enviar localizações de colaboradores.
public sealed class ReverseGeocodingService(HttpClient client, IConfiguration configuration)
{
    public bool IsConfigured => Uri.TryCreate(configuration["Geocoding:ReverseEndpoint"], UriKind.Absolute, out var uri)
        && uri.Scheme == "https" && !uri.Host.Equals("nominatim.openstreetmap.org", StringComparison.OrdinalIgnoreCase);

    public async Task<ApproximatePlace?> ResolveAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (!IsConfigured) return null;
        GeolocationValidationService.ValidateCoordinates(latitude, longitude);
        var endpoint = configuration["Geocoding:ReverseEndpoint"]!;
        var query = string.Create(CultureInfo.InvariantCulture, $"format=jsonv2&addressdetails=1&zoom=14&lat={latitude}&lon={longitude}&accept-language=pt-BR");
        using var response = await client.GetAsync(endpoint + (endpoint.Contains('?') ? "&" : "?") + query, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var json = JsonDocument.Parse(content);
        if (json.RootElement.ValueKind != JsonValueKind.Object) return null;
        if (!json.RootElement.TryGetProperty("address", out var address) || address.ValueKind != JsonValueKind.Object) return null;
        string? Read(params string[] keys) => keys.Select(k => address.TryGetProperty(k, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        var parts = new[] { Read("suburb", "neighbourhood", "quarter"), Read("city", "town", "village", "municipality"), Read("state"), Read("country") };
        var name = string.Join(", ", parts.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
        if (name.Length == 0) return null;
        var attribution = json.RootElement.TryGetProperty("licence", out var licence) && licence.ValueKind == JsonValueKind.String ? licence.GetString() ?? "" : "";
        return new ApproximatePlace(name[..Math.Min(500, name.Length)], attribution[..Math.Min(500, attribution.Length)]);
    }
}
