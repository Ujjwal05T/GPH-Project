namespace GPH.Models.DTOs
{
    public class BulkDeleteResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<int> DeletedBookIds { get; set; } = new List<int>();
    }
}
