// GPH/DTOs/ApproveExpenseDto.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;

public class ApproveExpenseDto
{
    [Required]
    public bool IsApproved { get; set; }
}