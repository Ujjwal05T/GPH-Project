// GPH/Controllers/CoachingCentersController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CoachingCentersController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public CoachingCentersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/coachingcenters
    [HttpGet]
    public async Task<IActionResult> GetAllCoachingCenters()
    {
        var centers = await _context.CoachingCenters
            .Select(c => new CoachingCenterDto
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                City = c.City
            })
            .ToListAsync();
        return Ok(centers);
    }

    // POST: /api/coachingcenters
    [HttpPost]
    public async Task<IActionResult> CreateCoachingCenter([FromBody] CreateCoachingCenterDto dto)
    {
        var newCenter = new CoachingCenter
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        _context.CoachingCenters.Add(newCenter);
        await _context.SaveChangesAsync();

        var resultDto = new CoachingCenterDto
        {
            Id = newCenter.Id,
            Name = newCenter.Name,
            Address = newCenter.Address,
            City = newCenter.City
        };

        return CreatedAtAction(nameof(GetCoachingCenterById), new { id = resultDto.Id }, resultDto);
    }

    // GET: /api/coachingcenters/{id} - Helper endpoint
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCoachingCenterById(int id)
    {
        var center = await _context.CoachingCenters.FindAsync(id);
        if (center == null) return NotFound();
        return Ok(center);
    }
}