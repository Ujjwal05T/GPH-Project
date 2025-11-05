public class UpdateTeacherDto
{
    public string Name { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
        public string? PrimarySubject { get; set; } // << --- YEH PROPERTY HONI CHAHIYE ---
    public string? ClassesTaught { get; set; } // << --- YEH PROPERTY HONI CHAHIYE ---

    // Add other fields to update later if needed
}