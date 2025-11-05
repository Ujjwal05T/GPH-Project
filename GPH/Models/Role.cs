// GPH/Models/Role.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class Role
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; // e.g., "Admin", "ASM", "Executive"
}