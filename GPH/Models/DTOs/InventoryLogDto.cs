namespace GPH.DTOs;
public class InventoryLogDto
{
    public DateTime Date { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Type { get; set; } = string.Empty; // "Distributed" ya "Ordered"
}