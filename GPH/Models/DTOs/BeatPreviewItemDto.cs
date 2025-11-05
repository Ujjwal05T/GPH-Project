// GPH/DTOs/BeatPreviewDto.cs
public class BeatPreviewItemDto
{
    public string LocationName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Exists in DB", "New (Found on Google)", "Error"
    public string? Details { get; set; }
}

public class BeatPreviewDto
{
    public List<BeatPreviewItemDto> Items { get; set; } = new();
    public int SuccessCount => Items.Count(i => i.Status != "Error");
    public int ErrorCount => Items.Count(i => i.Status == "Error");
}