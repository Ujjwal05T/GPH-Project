// GPH/Controllers/TeachersController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[ApiController]
[Route("api/[controller]")] // Route will be "/api/teachers"
public class TeachersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeachersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: /api/teachers
    [HttpPost]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherDto teacherDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // --- Validation Logic ---
        // Check if the school they are being assigned to actually exists.
        var schoolExists = await _context.Schools.AnyAsync(s => s.Id == teacherDto.SchoolId);
        if (!schoolExists)
        {
            // Return a specific, helpful error message
            return BadRequest(new { message = $"School with ID {teacherDto.SchoolId} not found." });
        }

        var newTeacher = new Teacher
        {
            Name = teacherDto.Name,
            WhatsAppNumber = teacherDto.WhatsAppNumber,
            PrimarySubject = teacherDto.PrimarySubject,
            SchoolId = teacherDto.SchoolId
        };

        _context.Teachers.Add(newTeacher);
        await _context.SaveChangesAsync();

        var resultDto = new TeacherDto
        {
            Id = newTeacher.Id,
            Name = newTeacher.Name,
            WhatsAppNumber = newTeacher.WhatsAppNumber,
            PrimarySubject = newTeacher.PrimarySubject,
            SchoolId = newTeacher.SchoolId
        };

        return CreatedAtAction(nameof(GetTeacherById), new { id = resultDto.Id }, resultDto);
    }

    // GET: /api/teachers
    [HttpGet]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _context.Teachers
            .Select(t => new TeacherDto
            {
                Id = t.Id,
                Name = t.Name,
                WhatsAppNumber = t.WhatsAppNumber,
                PrimarySubject = t.PrimarySubject,
                SchoolId = t.SchoolId
            })
            .ToListAsync();

        return Ok(teachers);
    }

    // GET: /api/teachers/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeacherById(int id)
    {
        var teacher = await _context.Teachers
            .Select(t => new TeacherDto
            {
                Id = t.Id,
                Name = t.Name,
                WhatsAppNumber = t.WhatsAppNumber,
                PrimarySubject = t.PrimarySubject,
                SchoolId = t.SchoolId
            })
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
        {
            return NotFound();
        }

        return Ok(teacher);
    }
    

    // GET: /api/schools/{schoolId}/teachers
    // A useful endpoint to get all teachers for a specific school
    [HttpGet("/api/schools/{schoolId}/teachers")]
    public async Task<IActionResult> GetTeachersBySchool(int schoolId)
    {
        var teachers = await _context.Teachers
            .Where(t => t.SchoolId == schoolId)
            .Select(t => new TeacherDto
            {
                Id = t.Id,
                Name = t.Name,
                WhatsAppNumber = t.WhatsAppNumber,
                PrimarySubject = t.PrimarySubject,
                SchoolId = t.SchoolId,
ClassesTaught = t.ClassesTaught,
            })
            .ToListAsync();

        return Ok(teachers);
    }
   // [HttpPut("{id}")]
// In GPH/Controllers/TeachersController.cs
[HttpPost("{id}/update")]

public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherDto dto) // <-- THIS IS THE FIX
{
    var teacher = await _context.Teachers.FindAsync(id);
    if (teacher == null) return NotFound();
    
    teacher.Name = dto.Name;
        teacher.WhatsAppNumber = dto.WhatsAppNumber ?? teacher.WhatsAppNumber;
teacher.PrimarySubject = dto.PrimarySubject!; // << --- YEH LINE ADD KAREIN ---
    teacher.ClassesTaught = dto.ClassesTaught;   // << --- YEH LINE ADD KAREIN ---
    
    await _context.SaveChangesAsync();
    return NoContent();
}
}