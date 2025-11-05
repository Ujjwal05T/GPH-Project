// GPH/Models/LocationPoint.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class LocationPoint
{
    public int Id { get; set; }

    [Required]
    public int DailyTrackingId { get; set; }
    public DailyTracking DailyTracking { get; set; } = null!;

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }
}