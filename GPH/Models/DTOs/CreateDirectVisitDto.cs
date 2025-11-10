// GPH/DTOs/CreateDirectVisitDto.cs
using Microsoft.AspNetCore.Http;
public class CreateDirectVisitDto
{
        public int SalesExecutiveId { get; set; }
    public int LocationType { get; set; } // 0=School, 1=Coaching, 2=Shop
    public string LocationName { get; set; } = string.Empty;
       public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Pincode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IFormFile Photo { get; set; } = null!;
    }