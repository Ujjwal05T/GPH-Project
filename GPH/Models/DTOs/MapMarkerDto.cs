using GPH.Models;

public class MapMarkerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}