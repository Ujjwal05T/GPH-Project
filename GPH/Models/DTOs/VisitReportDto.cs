// GPH/DTOs/VisitReportDto.cs
namespace GPH.DTOs;

public class VisitReportDto
{
    public DateTime VisitDate { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolArea { get; set; } = string.Empty;
}