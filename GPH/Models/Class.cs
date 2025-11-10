// GPH/Models/Class.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class Class
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ClassName { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
