// GPH/Models/School.cs

using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class School
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
     [MaxLength(200)]
    public string? AssignedArea { get; set; }


   [MaxLength(100)]
   public string City { get; set; } = string.Empty;
        [MaxLength(100)] // <-- ADD THIS PROPERTY
    public string? District { get; set; }

    [MaxLength(10)]
    public string Pincode { get; set; } = string.Empty;

    // Nullable because we might not know the principal's name initially
   
    [MaxLength(150)]
       public double? OfficialLatitude { get; set; } // Nullable, as it might not be set initially
    public double? OfficialLongitude { get; set; }
    public string? PrincipalName { get; set; }

    public int TotalStudentCount { get; set; }
   
    public string? StudentCounts { get; set; } // Storing as a JSON string: {"5th": 50, "6th": 60}
    [MaxLength(20)]
    public string? PrincipalMobileNumber { get; set; }
    // --- Navigation Property ---
    // This defines the one-to-many relationship: one School has many Teachers.
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public bool IsLocationVerified { get; set; } = false;
            public int? CreatedByExecutiveId { get; set; } // Nullable
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


}