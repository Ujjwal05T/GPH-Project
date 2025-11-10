// GPH/DTOs/RouteReplayDto.cs
namespace GPH.DTOs;



public class RouteReplayDto
{
    public int TrackingId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public decimal TotalDistanceKm { get; set; }
    public List<LocationPointDto> Path { get; set; } = new();
    public List<BeatPlanDto> PlannedVisits { get; set; } = new();
            public List<SessionDto> Sessions { get; set; } = new();


}