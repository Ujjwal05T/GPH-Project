// GPH/Models/DTOs/LocationVisitSummaryDto.cs
namespace GPH.DTOs;

public class LocationVisitSummaryDto
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public int TotalVisits { get; set; }
    public DateTime LastVisitDate { get; set; }
    public string LastVisitExecutive { get; set; } = string.Empty;
    public int LastVisitId { get; set; }
}

public class LocationVisitHistoryDto
{
    public int VisitId { get; set; }
    public DateTime VisitDate { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public int TeachersInteracted { get; set; }
    public int BooksDistributed { get; set; }
    public int OrdersPlaced { get; set; }
    public string? PrincipalRemarks { get; set; }
}
