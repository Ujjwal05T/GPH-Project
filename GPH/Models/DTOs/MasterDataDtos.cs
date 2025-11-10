// GPH/Models/DTOs/MasterDataDtos.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models.DTOs;

public class ClassDto
{
    public int Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class SubjectDto
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? SubjectCode { get; set; }
}

public class CreateClassDto
{
    [Required]
    [MaxLength(50)]
    public string ClassName { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}

public class CreateSubjectDto
{
    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? SubjectCode { get; set; }
}

public class CreateMappingDto
{
    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int ClassId { get; set; }
}
