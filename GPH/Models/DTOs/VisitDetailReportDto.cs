// GPH/DTOs/VisitDetailReportDto.cs
namespace GPH.DTOs;
// DTO for a single book distribution during the visit
public class BookDistributionDetailDto
{
    public string BookTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool WasRecommended { get; set; }
}
// DTO for a single order placed during the visit
public class OrderDetailDto
{
    public string BookTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
// DTO for interaction with a single teacher
public class TeacherInteractionDto
{
    public string TeacherName { get; set; } = string.Empty;
   public string? PrimarySubject { get; set; } // <-- YEH ADD KAREIN
    public string? ClassesTaught { get; set; }  // <-- YEH ADD KAREIN
    public string? WhatsAppNumber { get; set; } // <-- YEH ADD KAREIN
    public List<BookDistributionDetailDto> DistributedBooks { get; set; } = new();
    public List<OrderDetailDto> PlacedOrders { get; set; } = new();
}
// The Main DTO for the entire modal
public class VisitDetailReportDto
{
    // Basic Visit Info
    public int VisitId { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public DateTime VisitTimestamp { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    // Check-in Details
    public string? CheckInPhotoUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    // Principal's Meeting Details
    public string? ContactPersonLabel { get; set; } // e.g., "Principal", "Teacher", "Owner"
    public string? ContactPersonName { get; set; }
    public string? ContactPersonMobile { get; set; }
    public string? PrincipalRemarks { get; set; }
    public bool PermissionToMeetTeachers { get; set; }
    // Visit Tracking
    public int LocationVisitCount { get; set; } // Total visits to this location
    // Teacher Interactions
    public List<TeacherInteractionDto> TeacherInteractions { get; set; } = new();
}