// GPH/DTOs/CreateVisitDto.cs
using System.ComponentModel.DataAnnotations;
using GPH.Models;
using Microsoft.AspNetCore.Mvc;

namespace GPH.DTOs;

public class CreateVisitDto
{
    [Required]
    public int SalesExecutiveId { get; set; }

    [Required]
   public int LocationId { get; set; } // Use LocationId instead of SchoolId
    [Required]
    public LocationType LocationType { get; set; } // Specify the type
    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    // This property will be used to receive the uploaded photo
    [Required]
    public IFormFile CheckInPhoto { get; set; } = null!;
    
    public string? Notes { get; set; }
}