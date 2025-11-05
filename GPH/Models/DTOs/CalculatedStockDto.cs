// GPH/DTOs/CalculatedStockDto.cs
namespace GPH.DTOs;

public class CalculatedStockDto
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int TotalDistributed { get; set; }
    public int RemainingStock { get; set; }
}