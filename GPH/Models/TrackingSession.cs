//GPH/Models/TrackingSession.cs
using System.ComponentModel.DataAnnotations;
namespace GPH.Models;
public class TrackingSession
{
    public int Id { get; set; }
    [Required]
    public int DailyTrackingId { get; set; }
    public DailyTracking DailyTracking { get; set; } = null!;
    [Required]
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}