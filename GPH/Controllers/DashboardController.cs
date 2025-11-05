// // GPH/Controllers/DashboardController.cs

// using GPH.Data;
// using GPH.DTOs;
// using GPH.Helpers;
// using GPH.Models;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace GPH.Controllers;

// [Authorize(Roles = "Admin,ASM")]
// [Route("api/[controller]")]
// public class DashboardController : BaseApiController
// {
//     private readonly ApplicationDbContext _context;

//     public DashboardController(ApplicationDbContext context)
//     {
//         _context = context;
//     }

//     // GET: /api/dashboard/analytics
//     [HttpGet("analytics")]
//     public async Task<IActionResult> GetDashboardAnalytics()
//     {
//         var today = TimeZoneHelper.GetCurrentIstTime().Date;
//         var yesterday = today.AddDays(-1);

//         // --- KPI Card Calculations ---
//         var visitsToday = await _context.Visits.CountAsync(v => v.CheckInTimestamp.Date == today);
//         var visitsYesterday = await _context.Visits.CountAsync(v => v.CheckInTimestamp.Date == yesterday);

//         var expensesToday = await _context.Expenses.Where(e => e.ExpenseDate.Date == today).SumAsync(e => e.Amount);
//         var expensesYesterday = await _context.Expenses.Where(e => e.ExpenseDate.Date == yesterday).SumAsync(e => e.Amount);

//         var activeSalesmen = await _context.SalesExecutives.CountAsync(e => e.Status == UserStatus.Active);
//         var pendingApprovals = await _context.Expenses.CountAsync(e => !e.IsApproved);

//         var stockDistributedToday = await _context.BookDistributions
//             .Where(d => d.Visit.CheckInTimestamp.Date == today)
//             .SumAsync(d => d.Quantity);
//         var stockDistributedYesterday = await _context.BookDistributions
//             .Where(d => d.Visit.CheckInTimestamp.Date == yesterday)
//             .SumAsync(d => d.Quantity);

//         // --- Chart Data Calculations ---
//         var topSalesmen = await _context.Visits
//             .Where(v => v.CheckInTimestamp.Date == today)
//             .GroupBy(v => v.SalesExecutive.Name)
//             .Select(g => new ChartDataDto { Name = g.Key, Value = g.Count() })
//             .OrderByDescending(x => x.Value)
//             .Take(5)
//             .ToListAsync();

//        var areaDistribution = await _context.Visits
//     // First, filter for only school visits
//     .Where(v => v.CheckInTimestamp.Date == today && v.LocationType == LocationType.School)
//     // Then, join with the Schools table to get the area
//     .Join(_context.Schools,
//         visit => visit.LocationId,
//         school => school.Id,
//         (visit, school) => new { school.AssignedArea })
//     .Where(x => x.AssignedArea != null)
//     .GroupBy(x => x.AssignedArea)
//     .Select(g => new ChartDataDto { Name = g.Key!, Value = g.Count() })
//     .OrderByDescending(x => x.Value)
//     .ToListAsync();

//         // --- Assemble the Final DTO ---
//         var analytics = new DashboardAnalyticsDto
//         {
//             TotalVisits = CreateKpi("Total Visits Today", visitsToday, visitsYesterday),
//             TotalExpenses = CreateKpi("Total Expenses Claimed", expensesToday, expensesYesterday, isCurrency: true),
//             ActiveSalesmen = new KpiCardDto { Title = "Active Salesmen", Value = activeSalesmen.ToString() },
//             PendingApprovals = new KpiCardDto { Title = "Pending Approvals", Value = pendingApprovals.ToString() },
//             StockDistributed = CreateKpi("Stock Distributed", stockDistributedToday, stockDistributedYesterday),
//             TopPerformingSalesmen = topSalesmen,
//             AreaWiseVisitDistribution = areaDistribution
//         };

//         return Ok(analytics);
//     }
//     [HttpGet("asm-summary")]
// [Authorize(Roles = "ASM")]
// public async Task<IActionResult> GetAsmDashboardSummary()
// {
//     var asmId = CurrentUserId;
//     var today = TimeZoneHelper.GetCurrentIstTime().Date;
//     var yesterday = today.AddDays(-1);

//     // Get the list of executives managed by this ASM
//     var myTeamIds = await _context.SalesExecutives
//         .Where(e => e.ManagerId == asmId && e.Status == UserStatus.Active)
//         .Select(e => e.Id)
//         .ToListAsync();

//     if (!myTeamIds.Any())
//     {
//         // Handle case where ASM has no team members
//         return Ok(new AsmDashboardDto()); // Return empty dashboard
//     }

//     // --- Calculate how many team members completed their DA target today ---
//     int completedCount = 0;
//     foreach (var execId in myTeamIds)
//     {
//         var visits = await _context.Visits
//             .Where(v => v.SalesExecutiveId == execId && v.CheckInTimestamp.Date == today)
//             .Select(v => v.LocationType)
//             .ToListAsync();
        
//         if (visits.Count(vt => vt == LocationType.School) >= 4 &&
//             visits.Count(vt => vt == LocationType.CoachingCenter) >= 1 &&
//             visits.Count(vt => vt == LocationType.Shopkeeper) >= 1)
//         {
//             completedCount++;
//         }
//     }

//     // --- Calculate KPIs for the team ---
//     var visitsToday = await _context.Visits.CountAsync(v => myTeamIds.Contains(v.SalesExecutiveId) && v.CheckInTimestamp.Date == today);
//     var visitsYesterday = await _context.Visits.CountAsync(v => myTeamIds.Contains(v.SalesExecutiveId) && v.CheckInTimestamp.Date == yesterday);
    
//     var stockDistributedToday = await _context.BookDistributions
//         .Where(d => myTeamIds.Contains(d.Visit.SalesExecutiveId) && d.Visit.CheckInTimestamp.Date == today)
//         .SumAsync(d => d.Quantity);
//     // ... (add yesterday's stock calculation if needed)

//     // --- Calculate Charts for the team ---
//     var topSalesmen = await _context.Visits
//         .Where(v => myTeamIds.Contains(v.SalesExecutiveId) && v.CheckInTimestamp.Date == today)
//         .GroupBy(v => v.SalesExecutive.Name)
//         .Select(g => new ChartDataDto { Name = g.Key, Value = g.Count() })
//         .OrderByDescending(x => x.Value).Take(5).ToListAsync();
    
//     // ... (add area distribution chart logic if needed)

//     var summary = new AsmDashboardDto
//     {
//         TotalTeamMembers = myTeamIds.Count,
//         CompletedToday = completedCount,
//         TotalVisitsToday = CreateKpi("Total Visits Today", visitsToday, visitsYesterday),
//         ActiveSalesmen = new KpiCardDto { Title = "Active Salesmen", Value = myTeamIds.Count.ToString() },
//         StockDistributed = CreateKpi("Stock Distributed", stockDistributedToday, 0), // Simplified change %
//         TopPerformingSalesmen = topSalesmen
//     };

//     return Ok(summary);
// }

//     // Helper method to calculate percentage change for KPI cards
//     private KpiCardDto CreateKpi(string title, decimal currentValue, decimal previousValue, bool isCurrency = false)
//     {
//         var kpi = new KpiCardDto
//         {
//             Title = title,
//             Value = isCurrency ? $"₹{currentValue:N0}" : currentValue.ToString("N0")
//         };

//         if (previousValue > 0)
//         {
//             kpi.ChangePercentage = Math.Round(((double)currentValue - (double)previousValue) / (double)previousValue * 100, 1);
//             kpi.IsIncrease = kpi.ChangePercentage >= 0;
//         }
//         else if (currentValue > 0)
//         {
//             kpi.ChangePercentage = 100; // If previous was 0 and current is > 0, it's a 100% increase
//             kpi.IsIncrease = true;
//         }

//         return kpi;
//     }
    
// }
// GPH/Controllers/DashboardController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize(Roles = "Admin,ASM")]
[ApiController] // Add ApiController attribute
[Route("api/[controller]")]
public class DashboardController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/dashboard/analytics
    [HttpGet("analytics")]
    public async Task<IActionResult> GetDashboardAnalytics()
    {
        var today = TimeZoneHelper.GetCurrentIstTime().Date;
        var yesterday = today.AddDays(-1);

        var visitsToday = await _context.Visits.CountAsync(v => v.CheckInTimestamp.Date == today);
        var visitsYesterday = await _context.Visits.CountAsync(v => v.CheckInTimestamp.Date == yesterday);

        var expensesToday = await _context.Expenses.Where(e => e.ExpenseDate.Date == today).SumAsync(e => e.Amount);
        var expensesYesterday = await _context.Expenses.Where(e => e.ExpenseDate.Date == yesterday).SumAsync(e => e.Amount);

        var activeSalesmen = await _context.SalesExecutives.CountAsync(e => e.Status == UserStatus.Active);
        var pendingApprovals = await _context.Expenses.CountAsync(e => e.Status == ApprovalStatus.Pending);

        var stockDistributedToday = await _context.BookDistributions
            .Include(d => d.Visit) // Include Visit to filter by date
            .Where(d => d.Visit.CheckInTimestamp.Date == today)
            .SumAsync(d => d.Quantity);
        var stockDistributedYesterday = await _context.BookDistributions
            .Include(d => d.Visit)
            .Where(d => d.Visit.CheckInTimestamp.Date == yesterday)
            .SumAsync(d => d.Quantity);

        var topSalesmen = await _context.Visits
            .Include(v => v.SalesExecutive)
            .Where(v => v.CheckInTimestamp.Date == today)
            .GroupBy(v => v.SalesExecutive.Name)
            .Select(g => new ChartDataDto { Name = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync();

       var areaDistribution = await _context.Visits
            .Where(v => v.CheckInTimestamp.Date == today && v.LocationType == LocationType.School)
            .Join(_context.Schools, visit => visit.LocationId, school => school.Id, (visit, school) => new { school.AssignedArea })
            .Where(x => x.AssignedArea != null)
            .GroupBy(x => x.AssignedArea)
            .Select(g => new ChartDataDto { Name = g.Key!, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToListAsync();

        var analytics = new DashboardAnalyticsDto
        {
            TotalVisits = CreateKpi("Total Visits Today", visitsToday, visitsYesterday),
            TotalExpenses = CreateKpi("Total Expenses Claimed", expensesToday, expensesYesterday, isCurrency: true),
            ActiveSalesmen = new KpiCardDto { Title = "Active Salesmen", Value = activeSalesmen.ToString() },
            PendingApprovals = new KpiCardDto { Title = "Pending Approvals", Value = pendingApprovals.ToString() },
            StockDistributed = CreateKpi("Stock Distributed", stockDistributedToday, stockDistributedYesterday),
            TopPerformingSalesmen = topSalesmen,
            AreaWiseVisitDistribution = areaDistribution
        };

        return Ok(analytics);
    }

    [HttpGet("asm-summary")]
    [Authorize(Roles = "ASM")]
    public async Task<IActionResult> GetAsmDashboardSummary()
    {
        var asmId = CurrentUserId;
        var today = TimeZoneHelper.GetCurrentIstTime().Date;
        var yesterday = today.AddDays(-1);

        var myTeamIds = await _context.SalesExecutives
            .Where(e => e.ManagerId == asmId && e.Status == UserStatus.Active)
            .Select(e => e.Id)
            .ToListAsync();

        if (!myTeamIds.Any()) return Ok(new AsmDashboardDto());

        int completedCount = 0;
        foreach (var execId in myTeamIds)
        {
            var visits = await _context.Visits
                .Where(v => v.SalesExecutiveId == execId && v.CheckInTimestamp.Date == today)
                .Select(v => v.LocationType).ToListAsync();
            if (visits.Count(vt => vt == LocationType.School) >= 4 &&
                visits.Count(vt => vt == LocationType.CoachingCenter) >= 1 &&
                visits.Count(vt => vt == LocationType.Shopkeeper) >= 1)
            {
                completedCount++;
            }
        }

        var visitsToday = await _context.Visits.CountAsync(v => myTeamIds.Contains(v.SalesExecutiveId) && v.CheckInTimestamp.Date == today);
        var visitsYesterday = await _context.Visits.CountAsync(v => myTeamIds.Contains(v.SalesExecutiveId) && v.CheckInTimestamp.Date == yesterday);
        
        var stockDistributedToday = await _context.BookDistributions
            .Include(d => d.Visit)
            .Where(d => myTeamIds.Contains(d.Visit.SalesExecutiveId) && d.Visit.CheckInTimestamp.Date == today)
            .SumAsync(d => d.Quantity);

        var topSalesmen = await _context.Visits
            .Include(v => v.SalesExecutive)
            .Where(v => myTeamIds.Contains(v.SalesExecutiveId) && v.CheckInTimestamp.Date == today)
            .GroupBy(v => v.SalesExecutive.Name)
            .Select(g => new ChartDataDto { Name = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value).Take(5).ToListAsync();

        var summary = new AsmDashboardDto
        {
            TotalTeamMembers = myTeamIds.Count,
            CompletedToday = completedCount,
            TotalVisitsToday = CreateKpi("Total Visits Today", visitsToday, visitsYesterday),
            ActiveSalesmen = new KpiCardDto { Title = "Active Salesmen", Value = myTeamIds.Count.ToString() },
            StockDistributed = CreateKpi("Stock Distributed", stockDistributedToday, 0),
            TopPerformingSalesmen = topSalesmen
        };

        return Ok(summary);
    }

    [HttpGet("activity-feed")]
    public async Task<IActionResult> GetActivityFeed()
    {
        var recentVisitsQuery = await _context.Visits
            .Include(v => v.SalesExecutive)
            .OrderByDescending(v => v.CheckInTimestamp)
            .Take(5)
            .Select(v => new { v.CheckInTimestamp, v.LocationId, v.LocationType, ExecutiveName = v.SalesExecutive.Name })
            .ToListAsync();

        var schoolIds = recentVisitsQuery.Where(v => v.LocationType == LocationType.School).Select(v => v.LocationId).ToList();
        var coachingIds = recentVisitsQuery.Where(v => v.LocationType == LocationType.CoachingCenter).Select(v => v.LocationId).ToList();
        var shopkeeperIds = recentVisitsQuery.Where(v => v.LocationType == LocationType.Shopkeeper).Select(v => v.LocationId).ToList();

        var schools = await _context.Schools.Where(s => schoolIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);
        var coachings = await _context.CoachingCenters.Where(c => coachingIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);
        var shopkeepers = await _context.Shopkeepers.Where(s => shopkeeperIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);

        var recentVisits = recentVisitsQuery.Select(v => new ActivityFeedItemDto {
            Timestamp = v.CheckInTimestamp,
            EventType = "CheckIn",
            Description = $"{v.ExecutiveName} checked in at {v.LocationType switch {
                LocationType.School => schools.GetValueOrDefault(v.LocationId, "an unknown location"),
                LocationType.CoachingCenter => coachings.GetValueOrDefault(v.LocationId, "an unknown location"),
                LocationType.Shopkeeper => shopkeepers.GetValueOrDefault(v.LocationId, "an unknown location"),
                _ => "an unknown location"
            }}"
        }).ToList();

        var recentOrders = await _context.Orders
            .Include(o => o.Visit.SalesExecutive)
            .OrderByDescending(o => o.OrderDate).Take(5)
            .Select(o => new ActivityFeedItemDto {
                Timestamp = o.OrderDate,
                EventType = "OrderPlaced",
                Description = $"{o.Visit.SalesExecutive.Name} placed a new order."
            }).ToListAsync();

        var feed = recentVisits.Concat(recentOrders)
            .OrderByDescending(item => item.Timestamp)
            .Take(10)
            .ToList();
            
        return Ok(feed);
    }

    private KpiCardDto CreateKpi(string title, decimal currentValue, decimal previousValue, bool isCurrency = false)
    {
        var kpi = new KpiCardDto
        {
            Title = title,
            Value = isCurrency ? $"₹{currentValue:N0}" : currentValue.ToString("N0")
        };

        if (previousValue > 0)
        {
            kpi.ChangePercentage = Math.Round(((double)currentValue - (double)previousValue) / (double)previousValue * 100, 1);
            kpi.IsIncrease = kpi.ChangePercentage >= 0;
        }
        else if (currentValue > 0)
        {
            kpi.ChangePercentage = 100;
            kpi.IsIncrease = true;
        }

        return kpi;
    }
}