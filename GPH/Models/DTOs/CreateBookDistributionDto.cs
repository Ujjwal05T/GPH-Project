// GPH/DTOs/CreateBookDistributionDto.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;

public class CreateBookDistributionDto
{
    [Required]
    public int VisitId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Range(1, 100)] // Quantity must be between 1 and 100
    public int Quantity { get; set; } = 1;

    public bool WasRecommended { get; set; } = false;
}