public class ExecutiveStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VisitsCompleted { get; set; }
    public decimal KilometersTraveled { get; set; }
    public bool IsOnTrackForDA { get; set; } // This will power the red/green alert
}