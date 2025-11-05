// GPH/Controllers/LivePerformanceController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize(Roles = "Admin,ASM")]
[Route("api/[controller]")]
public class LivePerformanceController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public LivePerformanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/live-performance
    [HttpGet]
    public async Task<IActionResult> GetLivePerformanceFeed()
    {
        var today = TimeZoneHelper.GetCurrentIstTime().Date;

        // Get all executives, including their manager's name
        /* var executives = await _context.SalesExecutives
             .Include(e => e.Manager) // Include manager to get ASM name
             .Where(e => e.Role.Name == "Executive" || e.Role.Name == "ASM") // Filter for relevant roles
             .ToListAsync();
 */
 var executivesQuery = _context.SalesExecutives
        .Include(e => e.Manager)
        .Where(e => e.Role.Name == "Executive" || e.Role.Name == "ASM");

    // If the current user is an ASM, add another filter to the query
    if (CurrentUserRole == "ASM")
    {
        // Show the ASM themselves and the executives they manage
        executivesQuery = executivesQuery.Where(e => e.Id == CurrentUserId || e.ManagerId == CurrentUserId);
    }
    // Admins will not have this filter applied, so they will see everyone.
    
    var executives = await executivesQuery.ToListAsync();
        // Get all of today's tracking data in one go
        var todaysTracking = await _context.DailyTrackings
            .Include(t => t.LocationPoints)
            .Where(t => t.StartTime.Date == today)
            .ToDictionaryAsync(t => t.SalesExecutiveId);

        // Get all of today's visits in one go
        var todaysVisits = await _context.Visits
            .Where(v => v.CheckInTimestamp.Date == today)
            .GroupBy(v => v.SalesExecutiveId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        // Get all of today's expenses in one go
        var todaysExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate.Date == today)
            .GroupBy(e => e.SalesExecutiveId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(x => x.Amount));

        var liveData = new List<SalesmanLiveDataDto>();

        foreach (var exec in executives)
        {
            var data = new SalesmanLiveDataDto
            {
                Id = exec.Id,
                Name = exec.Name,
                AssignedArea = exec.AssignedArea ?? "N/A",
                AsmName = exec.Manager?.Name ?? "N/A"
            };

            if (todaysTracking.TryGetValue(exec.Id, out var tracking))
            {
                data.Status = "Active"; // Simplified status for now
                data.DistanceTravelled = tracking.TotalDistanceKm;
                
                var activeDuration = (tracking.EndTime ?? DateTime.UtcNow) - tracking.StartTime;
                data.ActiveHours = $"{activeDuration.Hours}h {activeDuration.Minutes}m";

                var lastPoint = tracking.LocationPoints.OrderByDescending(p => p.Timestamp).FirstOrDefault();
                if (lastPoint != null)
                {
                    data.LastUpdate = $"{TimeZoneHelper.ConvertUtcToIst(lastPoint.Timestamp):hh:mm tt}";
                    // In a real app, you'd use a reverse geocoding service here to get a location name
                    data.CurrentLocation = $"{lastPoint.Latitude:F4}, {lastPoint.Longitude:F4}";
                        data.Latitude = lastPoint.Latitude;
    data.Longitude = lastPoint.Longitude;
                }
            }
            
            if(todaysVisits.TryGetValue(exec.Id, out var visitCount))
            {
                data.VisitsCompleted = visitCount;
            }

            if(todaysExpenses.TryGetValue(exec.Id, out var expenseSum))
            {
                data.ExpensesToday = expenseSum;
            }

            liveData.Add(data);
        }

        // If the logged-in user is an ASM, filter the results to only their team
        if (CurrentUserRole == "ASM")
        {
            var asmName = executives.FirstOrDefault(e => e.Id == CurrentUserId)?.Name;
            liveData = liveData.Where(d => d.AsmName == asmName).ToList();
        }

        return Ok(liveData);
    }
}