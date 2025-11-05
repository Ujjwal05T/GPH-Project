namespace GPH.DTOs;
public class DetailedVisitDto
{
    public DateTime VisitDate { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string? PrincipalRemarks { get; set; }
}