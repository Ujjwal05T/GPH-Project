// GPH/DTOs/UpdateSchoolAndTeachersDto.cs
namespace GPH.DTOs;

public class TeacherInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string? ClassesTaught { get; set; }
    public string? PrimarySubject { get; set; }
    public string? WhatsAppNumber { get; set; }


}

public class UpdateSchoolAndTeachersDto
{
    public string PrincipalName { get; set; } = string.Empty;
    public string? PrincipalMobileNumber { get; set; }


    public int TotalStudentCount { get; set; }
    public List<TeacherInfoDto> Teachers { get; set; } = new();
}