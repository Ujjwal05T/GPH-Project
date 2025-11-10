// GPH/Models/SubjectClassMapping.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class SubjectClassMapping
{
    public int Id { get; set; }

    [Required]
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    [Required]
    public int ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
