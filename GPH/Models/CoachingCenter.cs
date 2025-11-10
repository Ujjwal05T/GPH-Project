// GPH/Models/CoachingCenter.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class CoachingCenter
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
     [MaxLength(500)]
    public string? Address { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)] // <-- ADD THIS
    public string? District { get; set; }
    [MaxLength(10)]
    public string? Pincode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
     [MaxLength(150)]
    public string? TeacherName { get; set; }

    [MaxLength(20)]
    public string? MobileNumber { get; set; }

    public int? StudentCount { get; set; }

    [MaxLength(200)]
    public string? Subjects { get; set; }

    [MaxLength(100)]
    public string? Classes { get; set; }
    public bool IsLocationVerified { get; set; } = false;
            public int? CreatedByExecutiveId { get; set; } // Nullable
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


}
    // We can add fields specific to coaching centers here later
