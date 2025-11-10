// GPH/Models/Subject.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class Subject
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? SubjectCode { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
