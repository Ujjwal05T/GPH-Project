// GPH/DTOs/CreateSchoolDto.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;
using GPH.Models; // Add this


public class CreateSchoolDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    // In DTOs/CreateSchoolDto.cs & DTOs/SchoolDto.cs
public string? AssignedArea { get; set; }

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Pincode { get; set; } = string.Empty;
    // In DTOs/CreateSchoolDto.cs & DTOs/SchoolDto.cs
public double? OfficialLatitude { get; set; }
public double? OfficialLongitude { get; set; }


    [MaxLength(150)]
    public string? PrincipalName { get; set; }

    public int TotalStudentCount { get; set; }
}