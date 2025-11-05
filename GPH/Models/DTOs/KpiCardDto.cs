// GPH/DTOs/KpiCardDto.cs
namespace GPH.DTOs;

public class KpiCardDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double ChangePercentage { get; set; }
    public bool IsIncrease { get; set; }
}