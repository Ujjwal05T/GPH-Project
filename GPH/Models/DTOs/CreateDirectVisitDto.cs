// GPH/DTOs/CreateDirectVisitDto.cs
using Microsoft.AspNetCore.Http;
public class CreateDirectVisitDto
{
    public string LocationName { get; set; } = string.Empty;
    public int LocationType { get; set; } // 0=School, 1=Coaching, 2=Shop
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int SalesExecutiveId { get; set; }
    public IFormFile Photo { get; set; } = null!;
    }
