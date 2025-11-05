// GPH/DTOs/InventoryAssignmentDto.cs
namespace GPH.DTOs;

public class InventoryAssignmentDto
{
    public int Id { get; set; }
    public int SalesExecutiveId { get; set; }
    public int BookId { get; set; }
    public int QuantityAssigned { get; set; }
    public DateTime DateAssigned { get; set; }
}