public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string ClassLevel { get; set; } = string.Empty;
    public string? Medium { get; set; } // Add karein
    public bool IsGift { get; set; }
    public decimal UnitPrice { get; set; } = 0;
}