// GPH/DTOs/OrderDto.cs
namespace GPH.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    
    // Details about the order
    public string BookTitle { get; set; } = string.Empty;
    public string BookSubject { get; set; } = string.Empty;
    public string BookClassLevel { get; set; } = string.Empty;
    public int Quantity { get; set; }

    // Details about who placed the order
    public string TeacherName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolArea { get; set; } = string.Empty;

    // Details about who generated the order
    public string ExecutiveName { get; set; } = string.Empty;
}