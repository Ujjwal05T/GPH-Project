// GPH/Controllers/SchoolsController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize]
[Route("api/[controller]")]
public class SchoolsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SchoolsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: /api/schools
    [HttpPost]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> CreateSchool([FromBody] CreateSchoolDto schoolDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newSchool = new School
        {
            Name = schoolDto.Name,
            Address = schoolDto.Address,
            AssignedArea = schoolDto.AssignedArea,
            City = schoolDto.City,
            Pincode = schoolDto.Pincode,
            PrincipalName = schoolDto.PrincipalName,
            TotalStudentCount = schoolDto.TotalStudentCount,
            OfficialLatitude = schoolDto.OfficialLatitude,
            OfficialLongitude = schoolDto.OfficialLongitude
        };

        _context.Schools.Add(newSchool);
        await _context.SaveChangesAsync();

        // It's good practice to return the created object
        var resultDto = new SchoolDto
        {
            Id = newSchool.Id,
            Name = newSchool.Name,
            Address = newSchool.Address,
            AssignedArea = newSchool.AssignedArea,
            City = newSchool.City,
            Pincode = newSchool.Pincode,
            PrincipalName = newSchool.PrincipalName,
            TotalStudentCount = newSchool.TotalStudentCount,
            OfficialLatitude = newSchool.OfficialLatitude,
            OfficialLongitude = newSchool.OfficialLongitude
        };

        return CreatedAtAction(nameof(GetSchoolById), new { id = resultDto.Id }, resultDto);
    }

    // GET: /api/schools
    [HttpGet]
    public async Task<IActionResult> GetAllSchools()
    {
        var schools = await _context.Schools
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                AssignedArea = s.AssignedArea,
                City = s.City,
                Pincode = s.Pincode,
                PrincipalName = s.PrincipalName,
                TotalStudentCount = s.TotalStudentCount,
                OfficialLatitude = s.OfficialLatitude,
                OfficialLongitude = s.OfficialLongitude
            })
            .ToListAsync();

        return Ok(schools);
    }

    // GET: /api/schools/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSchoolById(int id)
    {
        var school = await _context.Schools
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                AssignedArea = s.AssignedArea,
                City = s.City,
                Pincode = s.Pincode,
                PrincipalName = s.PrincipalName,
                TotalStudentCount = s.TotalStudentCount,
                OfficialLatitude = s.OfficialLatitude,
                OfficialLongitude = s.OfficialLongitude
            })
            .FirstOrDefaultAsync(s => s.Id == id);

        if (school == null)
        {
            return NotFound();
        }

        return Ok(school);
    }

    // PUT: /api/schools/{id}
   // [HttpPut("{id}")]
   [HttpPost("{id}/update")]
    public async Task<IActionResult> UpdateSchoolAndTeachers(int id, [FromBody] UpdateSchoolAndTeachersDto dto)
    {
        var schoolToUpdate = await _context.Schools.FindAsync(id);
        if (schoolToUpdate == null)
        {
            return NotFound();
        }

        schoolToUpdate.PrincipalName = dto.PrincipalName;
        schoolToUpdate.PrincipalMobileNumber = dto.PrincipalMobileNumber;
        schoolToUpdate.TotalStudentCount = dto.TotalStudentCount;

        if (dto.Teachers != null)
        {
            foreach (var teacherInfo in dto.Teachers)
            {
                var teacherExists = await _context.Teachers
                    .AnyAsync(t => t.Name.ToLower() == teacherInfo.Name.ToLower() && t.SchoolId == id);

                if (!teacherExists)
                {
                    var newTeacher = new Teacher
                    {
                        Name = teacherInfo.Name,
                        ClassesTaught = teacherInfo.ClassesTaught,
                        PrimarySubject = teacherInfo.PrimarySubject!,
                        SchoolId = id,
                        IsVerifiedByExecutive = false,
                        // Provide a default value for the non-nullable WhatsAppNumber
                        WhatsAppNumber = teacherInfo.WhatsAppNumber!,


                    };
                    _context.Teachers.Add(newTeacher);
                }
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

}