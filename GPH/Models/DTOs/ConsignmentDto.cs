// GPH/DTOs/ConsignmentDto.cs
using GPH.Models;
namespace GPH.DTOs;
public class ConsignmentDto
{
    public int Id { get; set; }
    public string TransportCompanyName { get; set; } = string.Empty;
    public string BiltyNumber { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty; // Salesman's name
    public ConsignmentStatus Status { get; set; }
    public DateTime DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public decimal? FreightCost { get; set; }
      public string? BiltyBillUrl { get; set; }
     public List<ConsignmentItemDto> Items { get; set; } = new();
}