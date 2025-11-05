using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;

public class CreateCoachingCenterDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
