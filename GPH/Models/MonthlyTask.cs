using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class MonthlyTask
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

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }
      [MaxLength(100)] // <-- ADD THIS BLOCK
    public string? Area { get; set; }
 [Required]
    public LocationType LocationType { get; set; } // <-- This is crucial
    [Required]
    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }
}