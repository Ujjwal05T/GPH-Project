// GPH/DTOs/BeatPlanDto.cs
using GPH.Models; // For LocationType and PlanStatus

namespace GPH.DTOs;

public class BeatPlanDto
{
    public int Id { get; set; }
    public int SalesExecutiveId { get; set; }

    // --- CORRECTED PROPERTIES ---
    // These are the generic properties that can hold info for a 
    // School, Coaching Center, or Shopkeeper.
    public int LocationId { get; set; }
    public LocationType LocationType { get; set; }
    public string LocationName { get; set; } = string.Empty;
 public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime PlanDate { get; set; }
    public PlanStatus Status { get; set; }
}
