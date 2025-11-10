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
     public async Task<SnappedPathResult?> SnapToRoadsAsync(List<LocationPointDto> rawPath)
    {
        if (string.IsNullOrEmpty(_apiKey) || rawPath.Count < 2)
        {
            return null;
        }

        // The Roads API has a limit of 100 points per request.
        // We must send the points in chunks if there are more than 100.
        var allSnappedPoints = new List<LocationPointDto>();
        var pathChunks = rawPath.Select((p, i) => new { Index = i, Point = p })
                                .GroupBy(x => x.Index / 100)
                                .Select(g => g.Select(x => x.Point).ToList());

        foreach (var chunk in pathChunks)
        {
            var pathString = string.Join("|", chunk.Select(p => $"{p.Latitude},{p.Longitude}"));
            var url = $"https://roads.googleapis.com/v1/snapToRoads?path={pathString}&interpolate=true&key={_apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Snap to Roads API failed with status: {response.StatusCode}");
                    return null; // If one chunk fails, we can't build the full path
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("snappedPoints", out var snappedPointsElement))
                {
                    foreach (var point in snappedPointsElement.EnumerateArray())
                    {
                        var location = point.GetProperty("location");
                        allSnappedPoints.Add(new LocationPointDto
                        {
                            Latitude = location.GetProperty("latitude").GetDouble(),
                            Longitude = location.GetProperty("longitude").GetDouble(),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred in SnapToRoadsAsync: {ex.Message}");
                return null; // Return null on failure
            }
        }

        if (allSnappedPoints.Count < 2)
        {
            return new SnappedPathResult { Path = rawPath, DistanceInMeters = 0 }; // Fallback to raw path if snapping fails
        }

        // Calculate the total distance of the snapped path
        decimal totalDistance = 0;
        for (int i = 0; i < allSnappedPoints.Count - 1; i++)
        {
            totalDistance += (decimal)CalculateHaversineDistance(allSnappedPoints[i], allSnappedPoints[i + 1]);
        }

        return new SnappedPathResult
        {
            Path = allSnappedPoints,
            DistanceInMeters = totalDistance
        };
    }

    private double CalculateHaversineDistance(LocationPointDto p1, LocationPointDto p2)
    {
        var R = 6371e3; // Earth's radius in metres
        var φ1 = p1.Latitude * Math.PI / 180;
        var φ2 = p2.Latitude * Math.PI / 180;
        var Δφ = (p2.Latitude - p1.Latitude) * Math.PI / 180;
        var Δλ = (p2.Longitude - p1.Longitude) * Math.PI / 180;

        var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                Math.Cos(φ1) * Math.Cos(φ2) *
                Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // in metres
    }
    // --- END OF NEW METHOD ---
}