namespace GPH.Services;

public class GeocodingResult
{
    public string Address { get; set; } = string.Empty; // The formatted address
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Accuracy { get; set; } = "UNKNOWN";
}

public interface IGeocodingService
{
    // Update the method signature to accept lat/lng string
    Task<GeocodingResult?> GetCoordinatesAsync(string latlng);
}