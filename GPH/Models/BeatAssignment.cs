// GPH/Models/BeatAssignment.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class BeatAssignment
{
    public int Id { get; set; }

    [Required]
    public int SalesExecutiveId { get; set; }
    public SalesExecutive SalesExecutive { get; set; } = null!;

    [Required]
    public DateTime AssignedMonth { get; set; }

    [Required]
    [MaxLength(200)]
    public string LocationName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Area { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }
     public int LocationId { get; set; }
    public LocationType LocationType { get; set; }

    // We can add a status here later to track completion
    public bool IsCompleted { get; set; } = false;
}