// GPH/Models/Shopkeeper.cs
using System.ComponentModel.DataAnnotations;

namespace GPH.Models;

public class Shopkeeper
{
    public int Id { get; set; }
    [MaxLength(500)]
    [Required]
    public string Name { get; set; } = string.Empty;    public string? Address { get; set; }
    [MaxLength(100)]
  public string? City { get; set; }
        [MaxLength(100)] // <-- ADD THIS
    public string? District { get; set; }
    [MaxLength(10)]
    public string? Pincode { get; set; }  
      public double? Latitude { get; set; }
    public double? Longitude { get; set; }
      [MaxLength(150)]
    public string? OwnerName { get; set; }

    [MaxLength(20)]
  public string? MobileNumber { get; set; }
  public StockStatus CurrentStockStatus { get; set; } = StockStatus.Unknown;

  public bool IsLocationVerified { get; set; } = false;
          public int? CreatedByExecutiveId { get; set; } // Nullable
          public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // We can add fields specific to shopkeepers here later (e.g., GST number)
}