// GPH/DTOs/CreateExpenseDto.cs
using GPH.Models;
using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;

public class CreateExpenseDto
{
    [Required]
    public int SalesExecutiveId { get; set; }

    [Required]
    public ExpenseType Type { get; set; } // 0=TA, 1=DA, 2=Other

    [Required]
    [Range(0.01, 100000.00)] // Amount must be positive
    public decimal Amount { get; set; }

    [Required]
    public DateTime ExpenseDate { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
    public IFormFile? BillFile { get; set; }

}