// GPH/DTOs/ExpenseDto.cs
using GPH.Models; // Needed for the ExpenseType enum

namespace GPH.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public int SalesExecutiveId { get; set; }
    public string SalesExecutiveName { get; set; } = string.Empty; // Added for the UI

    public ExpenseType Type { get; set; }
    public decimal Amount { get; set; } // Make Amount nullable
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public bool IsApproved { get; set; }
    public ApprovalStatus Status { get; set; } // Changed from IsApproved
        public string? BillUrl { get; set; }



}