// GPH/Models/ConsignmentItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GPH.Models;

public class ConsignmentItem
{
    public int Id { get; set; }

    [Required]
    public int ConsignmentId { get; set; }
    public Consignment Consignment { get; set; } = null!;

    [Required]
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }
}