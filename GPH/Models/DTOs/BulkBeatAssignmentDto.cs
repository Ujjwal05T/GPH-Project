// GPH/DTOs/BulkBeatAssignmentDto.cs
using System.ComponentModel.DataAnnotations;

public class BulkBeatAssignmentDto
{
    [Required]
    public int SalesExecutiveId { get; set; }
    [Required]
    public DateTime AssignedMonth { get; set; }
    [Required]
    public IFormFile File { get; set; } = null!;
}