// GPH/Models/InventoryAssignment.cs

using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class InventoryAssignment
{
    public int Id { get; set; }

    // --- Foreign Keys ---
    [Required]
    public int SalesExecutiveId { get; set; }
    public SalesExecutive SalesExecutive { get; set; } = null!;

    [Required]
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    // --- Details ---
    [Required]
    public int QuantityAssigned { get; set; }

    public DateTime DateAssigned { get; set; } = DateTime.UtcNow;
}