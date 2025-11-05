// GPH/Controllers/MonthlyBeatController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace GPH.Controllers;

[Authorize(Roles = "Admin")] // Only Admins can manage monthly beats
[Route("api/monthly-beat")]
public class MonthlyBeatController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public MonthlyBeatController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/monthly-beat?executiveId=13&month=2025-10-01
   [HttpGet]
public async Task<IActionResult> GetMonthlyBeat([FromQuery] int executiveId, [FromQuery] DateTime month)
{
    var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
    
    // --- THIS IS THE FIX ---
    // Select the correct properties from the BeatAssignment model
    var assignments = await _context.BeatAssignments
        .Where(a => a.SalesExecutiveId == executiveId && a.AssignedMonth == firstDayOfMonth)
        .Select(a => new {
            a.Id,
            a.LocationName,
            a.Area,
            a.District,
            a.Address
        })
        .ToListAsync();
    // --- END FIX ---
    
    return Ok(assignments);
}
    // POST: /api/monthly-beat
    
}