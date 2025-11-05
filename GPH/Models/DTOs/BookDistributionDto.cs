// GPH/DTOs/BookDistributionDto.cs
namespace GPH.DTOs;

public class BookDistributionDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public int TeacherId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public bool WasRecommended { get; set; }
}