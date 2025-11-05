// GPH/DTOs/ExpenseReportDto.cs
using GPH.Models;

namespace GPH.DTOs;

public class ExpenseReportDto
{
    public DateTime ExpenseDate { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public ExpenseType Type { get; set; }
    public decimal Amount { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Description { get; set; }
}