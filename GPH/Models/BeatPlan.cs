// GPH/Models/BeatPlan.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class BeatPlan
{
    public int Id { get; set; }

    [Required]
    public int SalesExecutiveId { get; set; }
    public SalesExecutive SalesExecutive { get; set; } = null!;

    [Required]
    public int LocationId { get; set; } 
    
    [Required]
    public LocationType LocationType { get; set; }

    [Required]
    public DateTime PlanDate { get; set; }

    [Required]
    public PlanStatus Status { get; set; } = PlanStatus.PendingApproval ;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}