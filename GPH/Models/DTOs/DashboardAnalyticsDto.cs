// GPH/DTOs/DashboardAnalyticsDto.cs
namespace GPH.DTOs;

public class DashboardAnalyticsDto
{
    public KpiCardDto TotalVisits { get; set; } = new();
    public KpiCardDto TotalExpenses { get; set; } = new();
    public KpiCardDto ActiveSalesmen { get; set; } = new();
    public KpiCardDto PendingApprovals { get; set; } = new();
    public KpiCardDto StockDistributed { get; set; } = new();
    public List<ChartDataDto> TopPerformingSalesmen { get; set; } = new();
    public List<ChartDataDto> AreaWiseVisitDistribution { get; set; } = new();
}