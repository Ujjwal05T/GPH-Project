// GPH/Models/Expense.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GPH.Models;

// An enum to define the types of expenses
public enum ExpenseType
{
    TravelAllowance, // TA
    DailyAllowance,  // DA
    Other
}

public class Expense
{
    public int Id { get; set; }

    // --- Foreign Key to SalesExecutive ---
    public int SalesExecutiveId { get; set; }
    public SalesExecutive SalesExecutive { get; set; } = null!;

    [Required]
    public ExpenseType Type { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")] // Use decimal for financial values
    public decimal Amount { get; set; }

    [Required]
    public DateTime ExpenseDate { get; set; }

    // Optional field for "Other" expenses
    [MaxLength(500)]
    public string? Description { get; set; }

    // For tracking the approval process
    public bool IsApproved { get; set; } = false;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public DateTime? ApprovedAt { get; set; }
    public string? BillUrl { get; set; }


}