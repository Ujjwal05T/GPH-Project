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
        [FromQuery] DateTime? month,
        [FromQuery] int? executiveId,
        [FromQuery] string? district,
        [FromQuery] string? area,
        [FromQuery] int? locationType)
    {
        // If no month specified, use current month
        var targetMonth = month ?? DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);

        // Query BeatAssignments table
        var beatQuery = _context.BeatAssignments
            .Include(a => a.SalesExecutive)
            .Where(a => a.AssignedMonth == firstDayOfMonth);

        if (executiveId.HasValue) { beatQuery = beatQuery.Where(a => a.SalesExecutiveId == executiveId.Value); }
        if (!string.IsNullOrEmpty(district)) { beatQuery = beatQuery.Where(a => a.District == district); }
        if (!string.IsNullOrEmpty(area)) { beatQuery = beatQuery.Where(a => a.Area == area); }
        if (locationType.HasValue) { beatQuery = beatQuery.Where(a => a.LocationType == (LocationType)locationType.Value); }
        if (CurrentUserRole == "ASM") { beatQuery = beatQuery.Where(a => a.SalesExecutive.ManagerId == CurrentUserId); }

        var beatAssignments = await beatQuery
            .Select(a => new BeatAssignmentDto
            {
                Id = a.Id,
                ExecutiveId = a.SalesExecutiveId,
                ExecutiveName = a.SalesExecutive.Name,
                LocationName = a.LocationName,
                Area = a.Area,
                District = a.District,
                Address = a.Address,
                IsCompleted = a.IsCompleted,
                LocationType = a.LocationType,
                AssignedMonth = a.AssignedMonth
            })
            .ToListAsync();

        // Query MonthlyTasks table (for Simple Task List Mode uploads)
        var taskQuery = _context.MonthlyTasks
            .Include(t => t.SalesExecutive)
            .Where(t => t.AssignedMonth == firstDayOfMonth);

        if (executiveId.HasValue) { taskQuery = taskQuery.Where(t => t.SalesExecutiveId == executiveId.Value); }
        if (!string.IsNullOrEmpty(district)) { taskQuery = taskQuery.Where(t => t.District == district); }
        if (!string.IsNullOrEmpty(area)) { taskQuery = taskQuery.Where(t => t.Area == area); }
        if (locationType.HasValue) { taskQuery = taskQuery.Where(t => t.LocationType == (LocationType)locationType.Value); }
        if (CurrentUserRole == "ASM") { taskQuery = taskQuery.Where(t => t.SalesExecutive.ManagerId == CurrentUserId); }

        var monthlyTasks = await taskQuery
            .Select(t => new BeatAssignmentDto
            {
                Id = t.Id,
                ExecutiveId = t.SalesExecutiveId,
                ExecutiveName = t.SalesExecutive.Name,
                LocationName = t.LocationName,
                Area = t.Area,
                District = t.District,
                Address = t.Address,
                IsCompleted = t.IsCompleted,
                LocationType = t.LocationType,
                AssignedMonth = t.AssignedMonth
            })
            .ToListAsync();

        // Merge both lists and sort
        var allAssignments = beatAssignments
            .Concat(monthlyTasks)
            .OrderBy(a => a.ExecutiveName)
            .ThenBy(a => a.LocationName)
            .ToList();

        return Ok(allAssignments);
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

    // GET: /api/beat-assignments/summary?month=2025-01-01
    [HttpGet("summary")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> GetAssignmentsSummary([FromQuery] DateTime? month)
    {
        var targetMonth = month ?? DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);

        // Query BeatAssignments
        var beatQuery = _context.BeatAssignments
            .Include(a => a.SalesExecutive)
            .Where(a => a.AssignedMonth == firstDayOfMonth);

        if (CurrentUserRole == "ASM")
        {
            beatQuery = beatQuery.Where(a => a.SalesExecutive.ManagerId == CurrentUserId);
        }

        var beatAssignments = await beatQuery.ToListAsync();

        // Query MonthlyTasks
        var taskQuery = _context.MonthlyTasks
            .Include(t => t.SalesExecutive)
            .Where(t => t.AssignedMonth == firstDayOfMonth);

        if (CurrentUserRole == "ASM")
        {
            taskQuery = taskQuery.Where(t => t.SalesExecutive.ManagerId == CurrentUserId);
        }

        var monthlyTasks = await taskQuery.ToListAsync();

        // Create a combined summary by grouping both sources
        var beatSummary = beatAssignments
            .GroupBy(a => new { a.SalesExecutiveId, a.SalesExecutive.Name })
            .Select(g => new
            {
                ExecutiveId = g.Key.SalesExecutiveId,
                ExecutiveName = g.Key.Name,
                TotalAssignments = g.Count(),
                Schools = g.Count(a => a.LocationType == LocationType.School),
                Coaching = g.Count(a => a.LocationType == LocationType.CoachingCenter),
                Shopkeepers = g.Count(a => a.LocationType == LocationType.Shopkeeper),
                Completed = g.Count(a => a.IsCompleted),
                Pending = g.Count(a => !a.IsCompleted)
            });

        var taskSummary = monthlyTasks
            .GroupBy(t => new { t.SalesExecutiveId, t.SalesExecutive.Name })
            .Select(g => new
            {
                ExecutiveId = g.Key.SalesExecutiveId,
                ExecutiveName = g.Key.Name,
                TotalAssignments = g.Count(),
                Schools = g.Count(t => t.LocationType == LocationType.School),
                Coaching = g.Count(t => t.LocationType == LocationType.CoachingCenter),
                Shopkeepers = g.Count(t => t.LocationType == LocationType.Shopkeeper),
                Completed = g.Count(t => t.IsCompleted),
                Pending = g.Count(t => !t.IsCompleted)
            });

        // Merge summaries by executive
        var combinedSummary = beatSummary
            .Concat(taskSummary)
            .GroupBy(s => new { s.ExecutiveId, s.ExecutiveName })
            .Select(g => new
            {
                ExecutiveId = g.Key.ExecutiveId,
                ExecutiveName = g.Key.ExecutiveName,
                TotalAssignments = g.Sum(s => s.TotalAssignments),
                Schools = g.Sum(s => s.Schools),
                Coaching = g.Sum(s => s.Coaching),
                Shopkeepers = g.Sum(s => s.Shopkeepers),
                Completed = g.Sum(s => s.Completed),
                Pending = g.Sum(s => s.Pending)
            })
            .OrderBy(s => s.ExecutiveName)
            .ToList();

        var totalAssignments = beatAssignments.Count + monthlyTasks.Count;

        return Ok(new
        {
            Month = firstDayOfMonth,
            TotalExecutives = combinedSummary.Count,
            TotalAssignments = totalAssignments,
            Summary = combinedSummary
        });
    }
}