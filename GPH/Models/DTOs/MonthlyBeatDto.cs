// GPH/DTOs/MonthlyBeatDto.cs
using System.ComponentModel.DataAnnotations;
using GPH.Models;

namespace GPH.DTOs;

public class AssignedLocationDto
{
    public int LocationId { get; set; }
    public LocationType LocationType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
}

public class UpdateMonthlyBeatDto
{
    [Required]
    public int SalesExecutiveId { get; set; }
    [Required]
    public DateTime AssignedMonth { get; set; }
    public List<AssignedLocationDto> Locations { get; set; } = new();
}