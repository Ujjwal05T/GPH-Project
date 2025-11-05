// GPH/Models/SalesExecutive.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GPH.Models;
public class SalesExecutive
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    // --- ADD NEW FIELDS ---
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty; // Dedicated username/login ID
    [MaxLength(500)]
    public string? Address { get; set; }
    [MaxLength(200)]
    public string? AssignedArea { get; set; }
  
    // --- END NEW FIELDS ---
    [Required]
    [MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    [Required]
    [Column(TypeName = "decimal(5, 2)")] // Allows for values like 999.99
    public decimal TaRatePerKm { get; set; } = 2.0m; // Default to 2.0
    [Required]

   [Column(TypeName = "decimal(18, 2)")]
  
    public decimal DaAmount { get; set; } = 300.0m; // Default to 300
    public int? ManagerId { get; set; }
    public SalesExecutive? Manager { get; set; }
    [Required]
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
  // --- PERSONAL DETAILS (YEH ADD KAREIN) ---
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(20)]
    public string? AlternatePhone { get; set; }
    [MaxLength(12)]
    public string? AadharNumber { get; set; }
    [MaxLength(10)]
    public string? PanNumber { get; set; }
    // --- BANK DETAILS (YEH BHI ADD KAREIN) ---
    [MaxLength(100)]
    public string? AccountHolderName { get; set; }
    [MaxLength(20)]
    public string? BankAccountNumber { get; set; }
    [MaxLength(100)]
    public string? BankName { get; set; }
    [MaxLength(100)]
    public string? BankBranch { get; set; }
    [MaxLength(11)]
    public string? IfscCode { get; set; }
}