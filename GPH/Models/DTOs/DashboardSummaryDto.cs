public class DashboardSummaryDto
{
    public int ActiveExecutives { get; set; }
    public int TotalVisitsToday { get; set; }
    public int PendingExpenses { get; set; }
    public decimal TotalExpensesToday { get; set; }
    public List<ExecutiveStatusDto> ExecutiveStatuses { get; set; } = new();
}