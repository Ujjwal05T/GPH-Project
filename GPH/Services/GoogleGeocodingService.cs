using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GPH.Services;

public class GoogleGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GoogleGeocodingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleApi:ApiKey"] ?? "";
        if (string.IsNullOrEmpty(_apiKey))
        {
            System.Console.WriteLine("WARNING: Google API Key is not configured.");
        }
    }

    public async Task<GeocodingResult?> GetCoordinatesAsync(string latlng)
    {
        if (string.IsNullOrEmpty(_apiKey)) return null;

        // The URL now uses 'latlng' instead of 'address' for reverse geocoding
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={Uri.EscapeDataString(latlng)}&key={_apiKey}";
        
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.GetProperty("status").GetString() == "OK")
            {
                var result = root.GetProperty("results")[0];
                var geometry = result.GetProperty("geometry");
                var location = geometry.GetProperty("location");
                
                return new GeocodingResult
                {
                    // We now get the formatted address from the first result
                    Address = result.GetProperty("formatted_address").GetString() ?? "Address not found",
                    Latitude = location.GetProperty("lat").GetDouble(),
                    Longitude = location.GetProperty("lng").GetDouble(),
                    Accuracy = geometry.GetProperty("location_type").GetString() ?? "UNKNOWN"
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"An exception occurred in GeocodingService: {ex.Message}");
            return null;
        }
    }
}