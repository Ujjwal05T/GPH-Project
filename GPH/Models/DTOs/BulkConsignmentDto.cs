// GPH/DTOs/BulkConsignmentDto.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GPH.DTOs;

public class BulkConsignmentDto
{
    [Required]
    public string TransportCompanyName { get; set; } = string.Empty;

    [Required]
    public string BiltyNumber { get; set; } = string.Empty;

    [Required]
    public DateTime DispatchDate { get; set; }

    [Required]
    public int SalesExecutiveId { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!; // The uploaded Excel file

    public IFormFile? BiltyBillFile { get; set; }
}