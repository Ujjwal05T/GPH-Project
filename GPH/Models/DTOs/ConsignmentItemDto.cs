// GPH/DTOs/ConsignmentItemDto.cs
namespace GPH.DTOs;
public class ConsignmentItemDto
{
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty; // Naya Add Karein
    public string BookSubject { get; set; } = string.Empty; // Naya Add Karein
    public string BookClassLevel { get; set; } = string.Empty; // Naya Add Karein
    public int Quantity { get; set; }
}