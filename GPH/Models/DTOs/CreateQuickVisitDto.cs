// GPH/DTOs/CreateQuickVisitDto.cs
using GPH.Models;
using Microsoft.AspNetCore.Http; // Added for IFormFile

public class CreateQuickVisitDto
{
    public string LocationName { get; set; } = string.Empty;
    public LocationType LocationType { get; set; }
    
    // CORRECT: Changed from double to string to match the form data
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public int SalesExecutiveId { get; set; }
    
    public IFormFile CheckInPhoto { get; set; } = null!;
            public int? BeatPlanId { get; set; }

    public Dictionary<string, string> Details { get; set; } = new();
}