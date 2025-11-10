using GPH.DTOs;
namespace GPH.Services;

public class GeocodingResult
{
    public string Address { get; set; } = string.Empty; // The formatted address
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Accuracy { get; set; } = "UNKNOWN";
}
public class SnappedPathResult
{
    public List<LocationPointDto> Path { get; set; } = new();
    public decimal DistanceInMeters { get; set; }
}

public interface IGeocodingService
{
    // Update the method signature to accept lat/lng string
    Task<GeocodingResult?> GetCoordinatesAsync(string latlng);
        Task<SnappedPathResult?> SnapToRoadsAsync(List<LocationPointDto> rawPath);

}