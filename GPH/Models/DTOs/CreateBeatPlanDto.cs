// GPH/DTOs/CreateBeatPlanDto.cs
using System.ComponentModel.DataAnnotations;
using GPH.Models;

namespace GPH.DTOs;

public class PlannedLocationDto
{
    // [Required]
    public int? LocationId { get; set; } // Make this nullable
    [Required]
    public LocationType LocationType { get; set; }
    public string? NewLocationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
          public string? Address { get; set; }
        public string? City { get; set; }
            public string? District { get; set; } // <-- ADD THIS LINE

        public string? Pincode { get; set; }
}
public class CreateBeatPlanDto
{
    [Required]
    public int SalesExecutiveId { get; set; }

    [Required]
    public DateTime PlanDate { get; set; }

    [Required]
    [MinLength(1)] // Must plan at least one visit
    public List<PlannedLocationDto> Locations { get; set; } = new();
}