// GPH/DTOs/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;

public class LoginDto
{
    [Required]
    public string MobileNumber { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}