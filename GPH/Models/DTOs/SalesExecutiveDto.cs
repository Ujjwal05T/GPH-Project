// GPH/DTOs/SalesExecutiveDto.cs
using GPH.Models;
namespace GPH.DTOs;
// GPH/DTOs/SalesExecutiveDto.cs
public class SalesExecutiveDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
        public int RoleId { get; set; } // <-- ADD THIS
    public string RoleName { get; set; } = string.Empty;
        public string? Address { get; set; } // <-- ADD THIS
    public string? AssignedArea { get; set; }
    public string MobileNumber { get; set; } = string.Empty; // Renamed from contactNumber
    public UserStatus Status { get; set; }
    public decimal TaRatePerKm { get; set; }
        public decimal DaAmount { get; set; }
         public int? ManagerId { get; set; }
          public string? ManagerName { get; set; }
    // --- YEH NAYI PROPERTY ADD KAREIN ---
    public string Password { get; set; } = string.Empty;
         public DateTime? DateOfBirth { get; set; }
    public string? AlternatePhone { get; set; }
    public string? AadharNumber { get; set; }
    public string? PanNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? IfscCode { get; set; }
}