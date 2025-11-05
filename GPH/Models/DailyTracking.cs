// GPH/Models/DailyTracking.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GPH.Models;

public class DailyTracking
{
    public int Id { get; set; }

    [Required]
    public int SalesExecutiveId { get; set; }
    public SalesExecutive SalesExecutive { get; set; } = null!;

    [Required]
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalDistanceKm { get; set; } = 0;

    // Navigation property to store all the location points for this day
        [MaxLength(500)] // A reasonable length for an address
    public string? LastKnownAddress { get; set; }
    public ICollection<LocationPoint> LocationPoints { get; set; } = new List<LocationPoint>();
}