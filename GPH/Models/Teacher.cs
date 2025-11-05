// GPH/Models/Teacher.cs

using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class Teacher
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public string? ClassesTaught { get; set; } // e.g., "9th, 10th"
    public bool IsVerifiedByExecutive { get; set; } = false; 
    

    [MaxLength(20)]
    public string WhatsAppNumber { get; set; } = string.Empty;

    // We will handle multiple subjects/classes later with a linking table.
    // For now, let's keep it simple.
    [MaxLength(100)]
    public string PrimarySubject { get; set; } = string.Empty;

    // --- Foreign Key Relationship ---
    public int SchoolId { get; set; } // The foreign key
    public School School { get; set; } = null!; // The navigation property back to the School
}