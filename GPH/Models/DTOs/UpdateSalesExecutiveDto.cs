// GPH/DTOs/UpdateSalesExecutiveDto.cs
using System.ComponentModel.DataAnnotations;
namespace GPH.DTOs;
public class UpdateSalesExecutiveDto
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Mobile Number is required.")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit Indian mobile number.")]
    public string MobileNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    [Required]
    public int RoleId { get; set; }
    public int? ManagerId { get; set; }
    [Required(ErrorMessage = "Assigned Area is required.")]
    [StringLength(200)]
    public string AssignedArea { get; set; } = string.Empty;
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string? Password { get; set; } // Optional on update
    [Required]
    [Range(0, 100)]
    public decimal TaRatePerKm { get; set; }
    [Required]
    [Range(0, 10000)]
    public decimal DaAmount { get; set; }
    // --- Personal Details with Validation ---
    public DateTime? DateOfBirth { get; set; }
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit alternate mobile number.")]
    public string? AlternatePhone { get; set; }
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhar Number must be exactly 12 digits.")]
    public string? AadharNumber { get; set; }
    [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Please enter a valid PAN number format (e.g., ABCDE1234F).")]
    public string? PanNumber { get; set; }
    // --- Bank Details with Validation ---
    [StringLength(100)]
    public string? AccountHolderName { get; set; }
    [RegularExpression(@"^\d{9,18}$", ErrorMessage = "Bank Account Number must be between 9 and 18 digits.")]
    public string? BankAccountNumber { get; set; }
    [StringLength(100)]
    public string? BankName { get; set; }
    [StringLength(100)]
    public string? BankBranch { get; set; }
    [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Please enter a valid 11-character IFSC code.")]
    public string? IfscCode { get; set; }
}