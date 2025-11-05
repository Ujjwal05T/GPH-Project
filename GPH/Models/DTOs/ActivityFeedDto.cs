public class ActivityFeedItemDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty; // "CheckIn", "OrderPlaced", "ExpenseApproved"
    public string Description { get; set; } = string.Empty;
}