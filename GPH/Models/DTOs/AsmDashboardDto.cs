// GPH/DTOs/AsmDashboardDto.cs
namespace GPH.DTOs;

// This DTO is a container for all the data needed by the ASM Dashboard
public class AsmDashboardDto
{
    // Data for the main performance overview card
    public int TotalTeamMembers { get; set; }
    public int CompletedToday { get; set; }
    public int DaEarnedPercentage => TotalTeamMembers > 0 ? (int)Math.Round((double)CompletedToday / TotalTeamMembers * 100) : 0;

    // Data for the smaller KPI cards
    public KpiCardDto TotalVisitsToday { get; set; } = new();
    public KpiCardDto ActiveSalesmen { get; set; } = new();
    public KpiCardDto StockDistributed { get; set; } = new();
    
    // Data for the charts
    public List<ChartDataDto> TopPerformingSalesmen { get; set; } = new();
    public List<ChartDataDto> AreaWiseVisitDistribution { get; set; } = new();
}