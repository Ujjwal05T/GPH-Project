namespace GPH.DTOs;

public class LiveLocationWithAddressDto
{
    public int SalesExecutiveId { get; set; }
    public string ExecutiveName { get; set; } = string.Empty;
    public string AsmName { get; set; } = "N/A";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Address { get; set; } = "Loading address..."; // New property
}