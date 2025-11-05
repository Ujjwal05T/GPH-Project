// GPH/DTOs/DailyActivityReportDto.cs
namespace GPH.DTOs;
// Ye ek single visit ki detail rakhega
public class VisitDetailForReportDto
{
    public DateTime CheckInTime { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
// Ye ek executive ki ek din ki poori summary rakhega
public class ExecutiveDailySummaryDto
{
    public int ExecutiveId { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int TotalVisits { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalTA { get; set; }
    public decimal TotalDA { get; set; }
    public decimal TotalOtherExpense { get; set; }
    public List<VisitDetailForReportDto> Visits { get; set; } = new();
}