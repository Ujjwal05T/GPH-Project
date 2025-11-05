// GPH/DTOs/TeacherDto.cs
namespace GPH.DTOs;

public class TeacherDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string PrimarySubject { get; set; } = string.Empty;
    public string? ClassesTaught { get; set; } 


    public int SchoolId { get; set; }
}