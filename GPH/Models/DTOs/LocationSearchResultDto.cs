// GPH/DTOs/LocationSearchResultDto.cs
using GPH.Models;

namespace GPH.DTOs;

public class LocationSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string TypeName => Type.ToString(); // e.g., "School", "Shopkeeper"
    public string? Address { get; set; }
     public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}