// GPH/Controllers/BeatAssignmentsController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize] // All methods require login
[ApiController]
[Route("api/beat-assignments")]
public class BeatAssignmentsController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public BeatAssignmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/beat-assignments (For Admin/ASM)
    [HttpGet]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] DateTime month,
        [FromQuery] int? executiveId,
        [FromQuery] string? district,
        [FromQuery] string? area)
    {
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var query = _context.BeatAssignments
            .Include(a => a.SalesExecutive)
            .Where(a => a.AssignedMonth == firstDayOfMonth);

        if (executiveId.HasValue) { query = query.Where(a => a.SalesExecutiveId == executiveId.Value); }
        if (!string.IsNullOrEmpty(district)) { query = query.Where(a => a.District == district); }
        if (!string.IsNullOrEmpty(area)) { query = query.Where(a => a.Area == area); }
        if (CurrentUserRole == "ASM") { query = query.Where(a => a.SalesExecutive.ManagerId == CurrentUserId); }

        var assignments = await query
            .OrderBy(a => a.SalesExecutive.Name).ThenBy(a => a.LocationName)
            .Select(a => new BeatAssignmentDto
            {
                Id = a.Id,
                ExecutiveName = a.SalesExecutive.Name,
                LocationName = a.LocationName,
                Area = a.Area,
                District = a.District,
                Address = a.Address,
                IsCompleted = a.IsCompleted
            })
            .ToListAsync();

        return Ok(assignments);
    }

    // GET: /api/beat-assignments/my-assignments (For Executive)
    [HttpGet("my-assignments")]
    [Authorize(Roles = "Executive")]
    public async Task<IActionResult> GetMyAssignments()
    {
        var executiveId = CurrentUserId;
        var today = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var assignments = await _context.BeatAssignments
            .Where(a => a.SalesExecutiveId == executiveId && a.AssignedMonth == firstDayOfMonth)
            .OrderBy(a => a.LocationName)
            .ToListAsync();

        // --- THIS IS THE FIX ---
        // Instead of ToDictionary, we will fetch the lists and handle duplicates gracefully.
        var schools = await _context.Schools.ToListAsync();
        var coachings = await _context.CoachingCenters.ToListAsync();
        var shopkeepers = await _context.Shopkeepers.ToListAsync();
        // --- END FIX ---

        var result = assignments.Select(a =>
        {
            if (string.IsNullOrEmpty(a.LocationName)) return null; // Skip if name is null

            bool hasCoordinates = false;
            int locationId = 0;

            switch (a.LocationType)
            {
                case LocationType.School:
                    // Find the FIRST matching school. This handles duplicates.
                    var school = schools.FirstOrDefault(s => s.Name.Equals(a.LocationName, StringComparison.OrdinalIgnoreCase));
                    if (school != null)
                    {
                        locationId = school.Id;
                        hasCoordinates = school.OfficialLatitude.HasValue;
                    }
                    break;
                case LocationType.CoachingCenter:
                    var coaching = coachings.FirstOrDefault(c => c.Name.Equals(a.LocationName, StringComparison.OrdinalIgnoreCase));
                    if (coaching != null)
                    {
                        locationId = coaching.Id;
                        hasCoordinates = coaching.Latitude.HasValue;
                    }
                    break;
                case LocationType.Shopkeeper:
                    var shop = shopkeepers.FirstOrDefault(s => s.Name.Equals(a.LocationName, StringComparison.OrdinalIgnoreCase));
                    if (shop != null)
                    {
                        locationId = shop.Id;
                        hasCoordinates = shop.Latitude.HasValue;
                    }
                    break;
            }

            return new
            {
                Id = locationId,
                Name = a.LocationName,
                Type = a.LocationType,
                HasCoordinates = hasCoordinates
            };
        }).Where(r => r != null).ToList();

        return Ok(result);
    }
// GET: /api/beat-assignments/suggestions?executiveId=13&month=2025-11-01
[HttpGet("suggestions")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetBeatSuggestions([FromQuery] int executiveId, [FromQuery] DateTime month)
{
    var previousMonth = new DateTime(month.Year, month.Month, 1).AddMonths(-1);

    // --- 1. Get all locations from the PREVIOUS month's assignment ---
    var previousAssignments = await _context.BeatAssignments
        .Where(a => a.SalesExecutiveId == executiveId && a.AssignedMonth == previousMonth)
        .Select(a => new { a.LocationId, a.LocationType, a.LocationName })
        .ToListAsync();

    // --- 2. Get all NEW locations discovered by this executive in the previous month ---
    // We find them by checking the creation date of the location itself.
    var discoveredSchools = await _context.Schools
        .Where(s => s.CreatedByExecutiveId == executiveId && s.CreatedAt >= previousMonth && s.CreatedAt < previousMonth.AddMonths(1))
        .Select(s => new { LocationId = s.Id, LocationType = LocationType.School, LocationName = s.Name })
        .ToListAsync();
    
    var discoveredCoachings = await _context.CoachingCenters
        .Where(c => c.CreatedByExecutiveId == executiveId && c.CreatedAt >= previousMonth && c.CreatedAt < previousMonth.AddMonths(1))
        .Select(c => new { LocationId = c.Id, LocationType = LocationType.CoachingCenter, LocationName = c.Name })
        .ToListAsync();

    var discoveredShops = await _context.Shopkeepers
        .Where(s => s.CreatedByExecutiveId == executiveId && s.CreatedAt >= previousMonth && s.CreatedAt < previousMonth.AddMonths(1))
        .Select(s => new { LocationId = s.Id, LocationType = LocationType.Shopkeeper, LocationName = s.Name })
        .ToListAsync();

    // --- 3. Combine the lists and remove duplicates ---
    var combinedList = previousAssignments
        .Concat(discoveredSchools)
        .Concat(discoveredCoachings)
        .Concat(discoveredShops)
        .GroupBy(x => new { x.LocationId, x.LocationType }) // Group to get unique locations
        .Select(g => g.First()) // Take the first instance of each unique location
        .OrderBy(x => x.LocationName)
        .ToList();

    // We need to enrich this with the full location details for the Admin's UI
    // For now, we'll return a simplified DTO.
    var result = combinedList.Select(x => new AssignedLocationDto
    {
        LocationId = x.LocationId,
        LocationType = x.LocationType,
        Name = x.LocationName
    }).ToList();

    return Ok(result);
}
}