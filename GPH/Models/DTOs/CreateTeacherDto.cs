// GPH/DTOs/CreateTeacherDto.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.DTOs;

public class CreateTeacherDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string WhatsAppNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PrimarySubject { get; set; } = string.Empty;

    [Required]
    public int SchoolId { get; set; } // The ID of the school this teacher belongs to
}