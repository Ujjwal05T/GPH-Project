// GPH/Models/Visit.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GPH.Models;

public class Visit
{
    public int Id { get; set; }

    // --- Foreign Key to SalesExecutive ---
    public int SalesExecutiveId { get; set; }
    public SalesExecutive SalesExecutive { get; set; } = null!;
 [Required]
    public int LocationId { get; set; } // The ID of the location
    [Required]
    public LocationType LocationType { get; set; } // The type of location (School, Coaching, Shop)

    // --- Foreign Key to School ---
   [Column(TypeName = "nvarchar(MAX)")]
    public string? PrincipalRemarks { get; set; } // For audio-to-text notes
    public bool PermissionToMeetTeachers { get; set; } = false;
    // --- Visit Details ---
    [Required]
    public DateTime CheckInTimestamp { get; set; } // Captured at the moment of photo check-in

    public DateTime? CheckOutTimestamp { get; set; } // Can be null if visit is still in progress

    // We will store the path or URL to the check-in photo
    [MaxLength(500)]
    public string CheckInPhotoUrl { get; set; } = string.Empty;

    // GPS coordinates captured at check-in
    [MaxLength(50)]
    public double Latitude { get; set; } 

    [MaxLength(50)]
    public double Longitude { get; set; } 
    
    // A place for the executive to add notes (optional)
    [Column(TypeName = "nvarchar(MAX)")]
    public string? Notes { get; set; }
 [Required]
    public VisitStatus Status { get; set; } = VisitStatus.InProgress;


    // Removed invalid implicit operator
}