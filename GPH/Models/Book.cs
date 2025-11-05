// GPH/Models/Book.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GPH.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Subject { get; set; } = string.Empty;

    // e.g., "Class 9", "Class 10", etc.
    [MaxLength(50)]
    public string ClassLevel { get; set; } = string.Empty;
    [MaxLength(50)] // << --- YEH NAYI PROPERTY ADD KAREIN ---
    public string? Medium { get; set; } // e.g., "Hindi Medium", "English Medium"

    // To distinguish between a sample book and a saleable one
    public bool IsSpecimen { get; set; } = true;
    public bool IsGift { get; set; } = false; // Default to false
         [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; } = 0; // Default to 0

}