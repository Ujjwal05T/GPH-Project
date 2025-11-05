// GPH/Models/Order.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GPH.Models;

public class Order
{
    public int Id { get; set; } // This is an INT

    // --- Foreign Keys ---
    public int VisitId { get; set; } // MUST BE AN INT
    public Visit Visit { get; set; } = null!;
        public int TeacherId { get; set; } // The teacher who placed the order
    public Teacher Teacher { get; set; } = null!;

    public int BookId { get; set; } // MUST BE AN INT
    public Book Book { get; set; } = null!;

    // --- Order Details ---
    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    // EF Core 7+ can handle calculated properties like this, but for simplicity
    // and to avoid any potential issues, let's make it a simple property.
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDeliveryDate { get; set; }
}