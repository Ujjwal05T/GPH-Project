// GPH/DTOs/ConsignmentPreviewDto.cs
namespace GPH.DTOs;

public class ParsedItemDto
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount => Quantity * UnitPrice;
}

public class ConsignmentPreviewDto
{
    public List<ParsedItemDto> SuccessItems { get; set; } = new();
    public List<string> ErrorMessages { get; set; } = new();
    public int TotalItemsFound => SuccessItems.Count + ErrorMessages.Count;
}