using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // IFormFile ke liye
namespace GPH.DTOs;
public class CreateConsignmentDto
{
    [Required]
    public string TransportCompanyName { get; set; } = string.Empty;
    [Required]
    public string BiltyNumber { get; set; } = string.Empty;
    [Required]
    public DateTime DispatchDate { get; set; }
    [Required]
    public int SalesExecutiveId { get; set; }
    // Hum items ko ab ek JSON string ke roop mein lenge
    [Required]
    public string ItemsJson { get; set; } = string.Empty;
    // File upload ke liye
    public IFormFile? BiltyBillFile { get; set; }
}