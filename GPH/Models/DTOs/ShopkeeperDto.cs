// GPH/DTOs/ShopkeeperDto.cs
namespace GPH.DTOs;

public class ShopkeeperDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
}