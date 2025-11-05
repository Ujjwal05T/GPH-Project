// GPH/DTOs/SchoolDto.cs
namespace GPH.DTOs;
using GPH.Models; // Add this

public class SchoolDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
        public string? AssignedArea { get; set; }

    public string City { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    public string? PrincipalName { get; set; }
    public int TotalStudentCount { get; set; }
        // --- ADD THESE TWO PROPERTIES ---
    public double? OfficialLatitude { get; set; }
    public double? OfficialLongitude { get; set; }

}