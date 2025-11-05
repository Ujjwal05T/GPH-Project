// GPH/Controllers/ReportsController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GPH.Helpers;
namespace GPH.Controllers;
[Authorize(Roles = "Admin,ASM")]
[Route("api/[controller]")]
public class ReportsController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }
    // GET: /api/reports/visits?startDate=...&endDate=...&executiveId=...
    [HttpGet("visits")]
    public async Task<IActionResult> GetVisitsReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? executiveId)
    {
        var query = _context.Visits
            .Include(v => v.SalesExecutive)
            .Where(v => v.CheckInTimestamp.Date >= startDate.Date && v.CheckInTimestamp.Date <= endDate.Date);
        if (executiveId.HasValue)
        {
            query = query.Where(v => v.SalesExecutiveId == executiveId.Value);
        }
        if (CurrentUserRole == "ASM")
        {
            query = query.Where(v => v.SalesExecutive != null && v.SalesExecutive.ManagerId == CurrentUserId);
        }
        // --- THIS IS THE CORRECTED QUERY LOGIC ---
        var reportData = await query
            .OrderByDescending(v => v.CheckInTimestamp)
            .Select(v => new
            {
        CheckInTimestamp = v.CheckInTimestamp, // Use the correct property name
                ExecutiveName = v.SalesExecutive.Name,
                v.LocationType,
                v.LocationId
            })
            .ToListAsync(); // Fetch the raw data first
        // Now, enrich the data with location names in memory
        var schoolIds = reportData.Where(r => r.LocationType == LocationType.School).Select(r => r.LocationId).ToList();
        var coachingIds = reportData.Where(r => r.LocationType == LocationType.CoachingCenter).Select(r => r.LocationId).ToList();
        var shopkeeperIds = reportData.Where(r => r.LocationType == LocationType.Shopkeeper).Select(r => r.LocationId).ToList();
        var schools = await _context.Schools.Where(s => schoolIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        var coachings = await _context.CoachingCenters.Where(c => coachingIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
        var shopkeepers = await _context.Shopkeepers.Where(s => shopkeeperIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        var finalReport = reportData.Select(r => new VisitReportDto
        {
    VisitDate = r.CheckInTimestamp, // Use the correct property name from the anonymous object
            ExecutiveName = r.ExecutiveName,
            SchoolName = r.LocationType switch // Using SchoolName as a generic LocationName
            {
                LocationType.School => schools.GetValueOrDefault(r.LocationId)?.Name ?? "N/A",
                LocationType.CoachingCenter => coachings.GetValueOrDefault(r.LocationId)?.Name ?? "N/A",
                LocationType.Shopkeeper => shopkeepers.GetValueOrDefault(r.LocationId)?.Name ?? "N/A",
                _ => "Unknown"
            },
            SchoolArea = r.LocationType == LocationType.School ? schools.GetValueOrDefault(r.LocationId)?.AssignedArea ?? "N/A" : "N/A"
        }).ToList();
        // --- END CORRECTED LOGIC ---
        return Ok(finalReport);
    }
    // GET: /api/reports/expenses?startDate=...&endDate=...&executiveId=...
    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpensesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? executiveId)
    {
        var query = _context.Expenses
            .Include(e => e.SalesExecutive)
            .Where(e => e.ExpenseDate.Date >= startDate.Date && e.ExpenseDate.Date <= endDate.Date);
        if (executiveId.HasValue)
        {
            query = query.Where(e => e.SalesExecutiveId == executiveId.Value);
        }
        if (CurrentUserRole == "ASM")
        {
            query = query.Where(e => e.SalesExecutive != null && e.SalesExecutive.ManagerId == CurrentUserId);
        }
        var reportData = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new ExpenseReportDto
            {
            ExpenseDate = TimeZoneHelper.ConvertUtcToIst(e.ExpenseDate),
                ExecutiveName = e.SalesExecutive.Name,
                Type = e.Type,
                Amount = e.Amount,
                Status = e.Status,
                Description = e.Description
            })
            .ToListAsync();
        return Ok(reportData);
    }
   // --- YEH NAYA ENDPOINT HAI: PERFORMANCE SUMMARY KE LIYE ---
   [HttpGet("performance-summary")]
public async Task<IActionResult> GetPerformanceSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? executiveId)
{
    var query = _context.SalesExecutives.AsQueryable();
    if (executiveId.HasValue)
    {
        query = query.Where(e => e.Id == executiveId.Value);
    }
    
    query = query.Where(e => e.RoleId == 2 || e.RoleId == 3);
    var summary = await query.Select(e => new PerformanceSummaryDto
    {
        ExecutiveId = e.Id,
        ExecutiveName = e.Name,
        RoleName = e.Role.Name,
PlannedVisits = _context.BeatPlans.Count(p => p.SalesExecutiveId == e.Id && p.PlanDate.Date >= startDate.Date && p.PlanDate.Date <= endDate.Date),
        TotalVisits = _context.Visits.Count(v => v.SalesExecutiveId == e.Id && v.CheckInTimestamp.Date >= startDate.Date && v.CheckInTimestamp.Date <= endDate.Date),
        TotalDistanceKm = _context.DailyTrackings.Where(t => t.SalesExecutiveId == e.Id && t.StartTime.Date >= startDate.Date && t.StartTime.Date <= endDate.Date).Sum(t => t.TotalDistanceKm),
        BooksDistributed = _context.BookDistributions.Count(bd => bd.Visit.SalesExecutiveId == e.Id && bd.Visit.CheckInTimestamp.Date >= startDate.Date && bd.Visit.CheckInTimestamp.Date <= endDate.Date),
        // --- YEH HAI NAYA LOGIC ---
        // Poora expense
        TotalExpenses = _context.Expenses.Where(ex => ex.SalesExecutiveId == e.Id && ex.ExpenseDate.Date >= startDate.Date && ex.ExpenseDate.Date <= endDate.Date).Sum(ex => ex.Amount),
        // Sirf TA
        TotalTA = _context.Expenses.Where(ex => ex.SalesExecutiveId == e.Id && ex.ExpenseDate.Date >= startDate.Date && ex.ExpenseDate.Date <= endDate.Date && ex.Type == ExpenseType.TravelAllowance).Sum(ex => ex.Amount),
        // Sirf DA
        TotalDA = _context.Expenses.Where(ex => ex.SalesExecutiveId == e.Id && ex.ExpenseDate.Date >= startDate.Date && ex.ExpenseDate.Date <= endDate.Date && ex.Type == ExpenseType.DailyAllowance).Sum(ex => ex.Amount),
        // Sirf Other
        OtherExpenses = _context.Expenses.Where(ex => ex.SalesExecutiveId == e.Id && ex.ExpenseDate.Date >= startDate.Date && ex.ExpenseDate.Date <= endDate.Date && ex.Type == ExpenseType.Other).Sum(ex => ex.Amount)
    }).ToListAsync();
    return Ok(summary);
}
    // --- YEH NAYA ENDPOINT HAI: DETAILED VISIT LOG KE LIYE ---
    [HttpGet("detailed-visits")]
    public async Task<IActionResult> GetDetailedVisits([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? executiveId)
    {
        var query = _context.Visits
            .Include(v => v.SalesExecutive)
            .Where(v => v.CheckInTimestamp.Date >= startDate.Date && v.CheckInTimestamp.Date <= endDate.Date);
        if (executiveId.HasValue)
        {
            query = query.Where(v => v.SalesExecutiveId == executiveId.Value);
        }
        var visits = await query.OrderByDescending(v => v.CheckInTimestamp).ToListAsync();
        
        // Location names ko efficiently fetch karein
        var schoolIds = visits.Where(v => v.LocationType == LocationType.School).Select(v => v.LocationId).Distinct().ToList();
        var schools = await _context.Schools.Where(s => schoolIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        // (Coaching aur Shopkeeper ke liye bhi yahan add kar sakte hain)
        var result = visits.Select(v => new DetailedVisitDto
        {
           VisitDate = TimeZoneHelper.ConvertUtcToIst(v.CheckInTimestamp),
            ExecutiveName = v.SalesExecutive.Name,
            LocationName = v.LocationType == LocationType.School ? schools.GetValueOrDefault(v.LocationId)?.Name ?? "N/A" : "Other",
            LocationType = v.LocationType.ToString(),
            Area = v.LocationType == LocationType.School ? schools.GetValueOrDefault(v.LocationId)?.AssignedArea ?? "N/A" : "N/A",
            PrincipalRemarks = v.PrincipalRemarks
        }).ToList();
        return Ok(result);
    }
    // --- YEH NAYA ENDPOINT HAI: INVENTORY LOG KE LIYE ---
    [HttpGet("inventory-log")]
    public async Task<IActionResult> GetInventoryLog([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? executiveId)
    {
        var distributionsQuery = _context.BookDistributions
            .Include(bd => bd.Visit.SalesExecutive)
            .Include(bd => bd.Book)
            .Include(bd => bd.Teacher)
            .Where(bd => bd.Visit.CheckInTimestamp.Date >= startDate.Date && bd.Visit.CheckInTimestamp.Date <= endDate.Date);
        if (executiveId.HasValue)
        {
            distributionsQuery = distributionsQuery.Where(bd => bd.Visit.SalesExecutiveId == executiveId.Value);
        }
        var distributions = await distributionsQuery.Select(bd => new InventoryLogDto
        {
            Date = bd.Visit.CheckInTimestamp,
            ExecutiveName = bd.Visit.SalesExecutive.Name,
            BookTitle = bd.Book.Title,
            TeacherName = bd.Teacher.Name,
            Quantity = bd.Quantity,
            Type = "Distributed"
        }).ToListAsync();
        // Yahan par Orders ka data bhi add kar sakte hain
        
        return Ok(distributions.OrderByDescending(d => d.Date));
    }
 [HttpGet("daily-activity")]
    public async Task<IActionResult> GetDailyActivityReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? executiveId)
    {
        // 1. Saara zaroori data ek saath fetch karein
        var visitsQuery = _context.Visits
            .Include(v => v.SalesExecutive)
            .Where(v => v.CheckInTimestamp.Date >= startDate.Date && v.CheckInTimestamp.Date <= endDate.Date);
        var expensesQuery = _context.Expenses
            .Where(e => e.ExpenseDate.Date >= startDate.Date && e.ExpenseDate.Date <= endDate.Date);
        var trackingQuery = _context.DailyTrackings
            .Where(t => t.StartTime.Date >= startDate.Date && t.StartTime.Date <= endDate.Date);
        if (executiveId.HasValue)
        {
            visitsQuery = visitsQuery.Where(v => v.SalesExecutiveId == executiveId.Value);
            expensesQuery = expensesQuery.Where(e => e.SalesExecutiveId == executiveId.Value);
            trackingQuery = trackingQuery.Where(t => t.SalesExecutiveId == executiveId.Value);
        }
        var allVisits = await visitsQuery.ToListAsync();
        var allExpenses = await expensesQuery.ToListAsync();
        var allTrackings = await trackingQuery.ToListAsync();
        // Location names ke liye helper dictionaries
        var schoolIds = allVisits.Where(v => v.LocationType == LocationType.School).Select(v => v.LocationId).Distinct().ToList();
        var schools = await _context.Schools.Where(s => schoolIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        // 2. Data ko (Date + Executive) ke hisaab se group karein
        var groupedByDayAndExec = allVisits.GroupBy(v => new { v.CheckInTimestamp.Date, v.SalesExecutiveId, v.SalesExecutive.Name });
        var result = new List<ExecutiveDailySummaryDto>();
        foreach (var group in groupedByDayAndExec)
        {
            var summary = new ExecutiveDailySummaryDto
            {
                Date = group.Key.Date,
                ExecutiveId = group.Key.SalesExecutiveId,
                ExecutiveName = group.Key.Name,
                TotalVisits = group.Count(),
                
                // Us din ka tracking record dhundhein
                TotalDistanceKm = allTrackings
                    .FirstOrDefault(t => t.SalesExecutiveId == group.Key.SalesExecutiveId && t.StartTime.Date == group.Key.Date)?.TotalDistanceKm ?? 0,
                
                // Us din ke expenses dhundhein
                TotalTA = allExpenses
                    .Where(e => e.SalesExecutiveId == group.Key.SalesExecutiveId && e.ExpenseDate.Date == group.Key.Date && e.Type == ExpenseType.TravelAllowance)
                    .Sum(e => e.Amount),
                TotalDA = allExpenses
                    .Where(e => e.SalesExecutiveId == group.Key.SalesExecutiveId && e.ExpenseDate.Date == group.Key.Date && e.Type == ExpenseType.DailyAllowance)
                    .Sum(e => e.Amount),
                TotalOtherExpense = allExpenses
                    .Where(e => e.SalesExecutiveId == group.Key.SalesExecutiveId && e.ExpenseDate.Date == group.Key.Date && e.Type == ExpenseType.Other)
                    .Sum(e => e.Amount),
                // Us din ki saari visits ki list banayein
                Visits = group.OrderBy(v => v.CheckInTimestamp).Select(v => new VisitDetailForReportDto
                {
                    CheckInTime = v.CheckInTimestamp,
                    LocationName = v.LocationType == LocationType.School ? schools.GetValueOrDefault(v.LocationId)?.Name ?? "N/A" : "Other Location",
                    LocationType = v.LocationType.ToString(),
                    Remarks = v.PrincipalRemarks
                }).ToList()
            };
            result.Add(summary);
        }
        return Ok(result.OrderByDescending(r => r.Date).ThenBy(r => r.ExecutiveName));
    }
}