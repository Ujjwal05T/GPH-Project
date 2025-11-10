// GPH/DTOs/DailyActivityDto.cs
namespace GPH.DTOs;
public class SessionDto
{
    public DateTime StartTimeIST { get; set; }
    public DateTime? EndTimeIST { get; set; }
    public string? Duration { get; set; }
}
public class DailyActivityDto
{
    public int ExecutiveId { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public string Status { get; set; } = "Not Started"; // "Active", "Completed"
    public List<SessionDto> Sessions { get; set; } = new();
    public string TotalDuration { get; set; } = "0h 0m";
}