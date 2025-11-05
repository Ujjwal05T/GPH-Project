// GPH/DTOs/LiveLocationDto.cs
namespace GPH.DTOs;

public class LiveLocationDto
{
    public int SalesExecutiveId { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
        public string AsmName { get; set; } = "N/A"; // <-- ADD THIS

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LastUpdated { get; set; }
}