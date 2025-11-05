// GPH/DTOs/ConsignmentDetailDto.cs
public class ConsignmentItemDetailDto
{
    public string BookTitle { get; set; } = string.Empty;
    public string BookSubject { get; set; } = string.Empty;
    public string BookClassLevel { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ConsignmentDetailDto
{
    public int Id { get; set; }
    public string TransportCompanyName { get; set; } = string.Empty;
    public string BiltyNumber { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime DispatchDate { get; set; }
    public List<ConsignmentItemDetailDto> Items { get; set; } = new();
}