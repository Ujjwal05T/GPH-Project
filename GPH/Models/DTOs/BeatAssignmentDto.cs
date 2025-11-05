// GPH/DTOs/BeatAssignmentDto.cs
namespace GPH.DTOs;

public class BeatAssignmentDto
{
    public int Id { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public bool IsCompleted { get; set; }
}