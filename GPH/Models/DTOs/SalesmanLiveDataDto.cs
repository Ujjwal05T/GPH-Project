// GPH/DTOs/SalesmanLiveDataDto.cs
using GPH.Models;

namespace GPH.DTOs;

public class SalesmanLiveDataDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssignedArea { get; set; } = string.Empty;
    public string AsmName { get; set; } = "N/A";
    public string Status { get; set; } = "Day Not Started"; // e.g., "Active", "Idle", "Offline"
    public int VisitsCompleted { get; set; }
    public int TargetVisits { get; set; } = 6; // Can be made dynamic later
    public string CurrentLocation { get; set; } = "Unknown";
    public decimal ExpensesToday { get; set; }
    public decimal DistanceTravelled { get; set; }
    public string ActiveHours { get; set; } = "0h 0m";
    public string LastUpdate { get; set; } = "N/A";
    public double Efficiency => TargetVisits > 0 ? (double)VisitsCompleted / TargetVisits * 100 : 0;
       public double Latitude { get; set; }
    public double Longitude { get; set; }
}