namespace GPH.DTOs;
// This DTO will hold all the information from the last visit to a school
public class LastVisitDetailsDto
{
    // Details from the School table itself
    public string? PrincipalName { get; set; }
    public string? PrincipalMobileNumber { get; set; }
    public int TotalStudentCount { get; set; }
    // Details from the last Visit record
    public string? PrincipalRemarks { get; set; }
    public bool PermissionToMeetTeachers { get; set; }
    // The list of all known teachers for that school
    public List<TeacherDto> KnownTeachers { get; set; } = new();
}