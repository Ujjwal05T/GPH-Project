public class CreateOrderDto
{
    public int VisitId { get; set; }
    public int TeacherId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}