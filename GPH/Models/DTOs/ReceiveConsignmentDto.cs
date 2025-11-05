// GPH/DTOs/ReceiveConsignmentDto.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GPH.DTOs;

public class ReceiveConsignmentDto
{
    [Required]
    [Range(0, 100000)]
    public decimal FreightCost { get; set; }

    // Add this property for the file upload
    public IFormFile? BiltyBillPhoto { get; set; }
}