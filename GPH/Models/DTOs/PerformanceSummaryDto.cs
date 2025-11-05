namespace GPH.DTOs;
public class PerformanceSummaryDto
{
    public int ExecutiveId { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
  public string RoleName { get; set; } = string.Empty;
        public int PlannedVisits { get; set; }
    public int TotalVisits { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalExpenses { get; set; }
    public int BooksDistributed { get; set; }
      public decimal TotalTA { get; set; } // Travel Allowance ka total
    public decimal TotalDA { get; set; } // Daily Allowance ka total
    public decimal OtherExpenses { get; set; } // Other expenses ka total
    }
