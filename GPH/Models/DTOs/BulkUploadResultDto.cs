namespace GPH.Models.DTOs
{
    public class BulkUploadResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<BookDto> SuccessfulBooks { get; set; } = new List<BookDto>();
    }
}
