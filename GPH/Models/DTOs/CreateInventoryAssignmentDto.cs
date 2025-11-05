// GPH/DTOs/CreateInventoryAssignmentDto.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;

public class CreateInventoryAssignmentDto
{
    [Required]
    public int SalesExecutiveId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    [Range(1, 5000)]
    public int QuantityAssigned { get; set; }
}