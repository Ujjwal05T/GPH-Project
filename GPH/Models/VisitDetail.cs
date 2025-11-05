// GPH/Models/VisitDetail.cs
namespace GPH.Models;
public class VisitDetail
{
    public int Id { get; set; }
    public int VisitId { get; set; } // Foreign key to the Visit
    public Visit Visit { get; set; } = null!;
    public string Key { get; set; } = string.Empty; // e.g., "StockStatus", "Feedback"
    public string Value { get; set; } = string.Empty; // e.g., "Low", "Good feedback..."
}