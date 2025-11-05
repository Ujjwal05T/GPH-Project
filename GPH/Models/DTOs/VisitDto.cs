// GPH/DTOs/VisitDto.cs
using GPH.Models;

namespace GPH.DTOs;

public class VisitDto
{
    public int Id { get; set; }
    public int SalesExecutiveId { get; set; }
     public int LocationId { get; set; }
    public LocationType LocationType { get; set; }
    public string LocationName { get; set; } = string.Empty; // Add this to show the name in the UI
    public DateTime CheckInTimestamp { get; set; }
        public DateTime CheckInTimestampIst { get; set; }

    public string CheckInPhotoUrl { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Notes { get; set; }
}