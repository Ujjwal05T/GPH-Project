// GPH/Models/Consignment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GPH.Models;
public class Consignment
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string TransportCompanyName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string BiltyNumber { get; set; } = string.Empty;
    public DateTime DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? FreightCost { get; set; }
    [Required]
    public int SalesExecutiveId { get; set; } // The executive it's assigned to
    public SalesExecutive SalesExecutive { get; set; } = null!;
    [Required]
    public ConsignmentStatus Status { get; set; } = ConsignmentStatus.InTransit;
    public string? BiltyBillUrl { get; set; }
    // Navigation property for all the books in this consignment
    public ICollection<ConsignmentItem> Items { get; set; } = new List<ConsignmentItem>();
}