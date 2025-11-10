// GPH/Controllers/ReportsController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GPH.Helpers;
namespace GPH.Controllers;

[Authorize] // Allow all authenticated users; specific roles defined per endpoint
[Route("api/[controller]")]
public class ReportsController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }
    // Helper method to apply pagination
    private async Task<PaginatedResponseDto<T>> CreatePaginatedResponse<T>(IQueryable<T> query, int pageNumber, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedResponseDto<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
    // GET: /api/reports/visits?startDate=...&endDate=...&executiveId=...
    [HttpGet("visits")]
    [Authorize(Roles = "Admin,ASM")]
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
    [Authorize(Roles = "Admin,ASM")]
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
    [Authorize(Roles = "Admin,ASM")]
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
    [Authorize(Roles = "Admin,ASM")]
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
            Id = v.Id,
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
    [Authorize(Roles = "Admin,ASM")]
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
    [Authorize(Roles = "Admin,ASM")]
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

    // GPH/Controllers/ReportsController.cs
    [HttpGet("workday-summary")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> GetWorkdaySummaryReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? executiveId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 15)

    {
        var query = _context.DailyTrackings
            .Include(t => t.SalesExecutive)
            .Include(t => t.TrackingSessions)
            .Where(t => t.StartTime.Date >= startDate.Date && t.StartTime.Date <= endDate.Date);
        if (executiveId.HasValue)
        {
            query = query.Where(t => t.SalesExecutiveId == executiveId.Value);
        }
        if (CurrentUserRole == "ASM")
        {
            query = query.Where(t => t.SalesExecutive.ManagerId == CurrentUserId);
        }

        // --- YEH HAI NAYA PAGINATION LOGIC ---
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var reportData = await query
            .OrderByDescending(t => t.StartTime)
            .Skip((pageNumber - 1) * pageSize) // Skip records for previous pages
            .Take(pageSize) // Take only records for the current page
            .ToListAsync();
        // --- END PAGINATION LOGIC ---
        var finalReport = reportData.Select(trackingRecord =>
        {

            return new
            {
                Date = TimeZoneHelper.ConvertUtcToIst(trackingRecord.StartTime).ToString("dd-MM-yyyy"), // Format change
                Name = trackingRecord.SalesExecutive.Name,
                StartTime = trackingRecord.TrackingSessions.Any()
                    ? TimeZoneHelper.ConvertUtcToIst(trackingRecord.TrackingSessions.Min(s => s.StartTime)).ToString("hh:mm tt")
                    : "N/A",
                EndTime = trackingRecord.EndTime.HasValue
                    ? TimeZoneHelper.ConvertUtcToIst(trackingRecord.EndTime.Value).ToString("hh:mm tt")
                    : "Active",
                trackingRecord.TotalDistanceKm,
                Sessions = trackingRecord.TrackingSessions.OrderBy(s => s.StartTime).Select(s => new
                {
                    SessionStart = TimeZoneHelper.ConvertUtcToIst(s.StartTime).ToString("hh:mm tt"),
                    SessionEnd = s.EndTime.HasValue ? TimeZoneHelper.ConvertUtcToIst(s.EndTime.Value).ToString("hh:mm tt") : "Active",
                    // SessionDuration yahan se hata diya gaya hai
                }).ToList()
            };
        });
        // Ek naye PaginatedResponseDto mein data wrap karke bhejein
        var paginatedResponse = new PaginatedResponseDto<object>
        {
            Items = finalReport.Cast<object>().ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalCount = totalCount
        };
        return Ok(paginatedResponse);
    }

    [HttpGet("visit-details/{visitId}")]
    public async Task<IActionResult> GetVisitDetailReport(int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.SalesExecutive)
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit == null)
        {
            return NotFound(new { message = "Visit not found." });
        }

        // Security check: Executives can only view their own visits; Admin/ASM can view any
        if (CurrentUserRole == "Executive" && visit.SalesExecutiveId != CurrentUserId)
        {
            return Forbid();
        }

        // 1. Location ka naam fetch karein
        string locationName = "Unknown Location";
        string? contactPersonLabel = null;
        string? contactPersonName = null;
        string? contactPersonMobile = null;

        switch (visit.LocationType)
        {
            case LocationType.School:
                var school = await _context.Schools.FindAsync(visit.LocationId);
                if (school != null)
                {
                    locationName = school.Name;
                    contactPersonLabel = "Principal";
                    contactPersonName = school.PrincipalName;
                    contactPersonMobile = school.PrincipalMobileNumber;
                }
                break;
            case LocationType.CoachingCenter:
                var coaching = await _context.CoachingCenters.FindAsync(visit.LocationId);
                if (coaching != null)
                {
                    locationName = coaching.Name;
                    contactPersonLabel = "Teacher/Contact";
                    contactPersonName = coaching.TeacherName;
                    contactPersonMobile = coaching.MobileNumber;
                }
                break;
            case LocationType.Shopkeeper:
                var shop = await _context.Shopkeepers.FindAsync(visit.LocationId);
                if (shop != null)
                {
                    locationName = shop.Name;
                    contactPersonLabel = "Owner/Shopkeeper";
                    contactPersonName = shop.OwnerName;
                    contactPersonMobile = shop.MobileNumber;
                }
                break;
        }

        // 2. Teacher interactions fetch karein
        var distributions = await _context.BookDistributions
            .Include(d => d.Book)
            .Include(d => d.Teacher)
            .Where(d => d.VisitId == visitId)
            .ToListAsync();

        var orders = await _context.Orders
            .Include(o => o.Book)
            .Include(o => o.Teacher)
            .Where(o => o.VisitId == visitId)
            .ToListAsync();

        var allTeacherIds = distributions.Select(d => d.TeacherId)
            .Union(orders.Select(o => o.TeacherId))
            .Distinct()
            .ToList();

        var teacherInteractions = new List<TeacherInteractionDto>();

        var teachers = await _context.Teachers
            .Where(t => allTeacherIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id);

        foreach (var teacherId in allTeacherIds)
        {
            if (teachers.TryGetValue(teacherId, out var teacher))
            {
                var interaction = new TeacherInteractionDto
                {
                    TeacherName = teacher.Name,
                    PrimarySubject = teacher.PrimarySubject, // <-- YEH ADD KAREIN
                    ClassesTaught = teacher.ClassesTaught,   // <-- YEH ADD KAREIN
                    WhatsAppNumber = teacher.WhatsAppNumber, // <-- YEH ADD KAREIN
                    DistributedBooks = distributions
                        .Where(d => d.TeacherId == teacherId)
                        .Select(d => new BookDistributionDetailDto
                        {
                            BookTitle = d.Book.Title,
                            Quantity = d.Quantity,
                            WasRecommended = d.WasRecommended
                        }).ToList(),
                    PlacedOrders = orders
                        .Where(o => o.TeacherId == teacherId)
                        .Select(o => new OrderDetailDto
                        {
                            BookTitle = o.Book.Title,
                            Quantity = o.Quantity
                        }).ToList()
                };
                teacherInteractions.Add(interaction);
            }
        }

        // 3. Final DTO assemble karein
        var resultDto = new VisitDetailReportDto
        {
            VisitId = visit.Id,
            ExecutiveName = visit.SalesExecutive.Name,
            VisitTimestamp = TimeZoneHelper.ConvertUtcToIst(visit.CheckInTimestamp),
            LocationName = locationName,
            LocationType = visit.LocationType.ToString(),
            CheckInPhotoUrl = visit.CheckInPhotoUrl,
            Latitude = visit.Latitude,
            Longitude = visit.Longitude,
            ContactPersonLabel = contactPersonLabel, // Nayi property
            ContactPersonName = contactPersonName,   // Nayi property
            ContactPersonMobile = contactPersonMobile, // Nayi property
            PrincipalRemarks = visit.PrincipalRemarks,
            PermissionToMeetTeachers = visit.PermissionToMeetTeachers,
            TeacherInteractions = teacherInteractions
        };

        return Ok(resultDto);
    }

// === YEH NAYA METHOD ADD KAREIN ===
[HttpPost("bulk-visit-details")]
public async Task<IActionResult> GetBulkVisitDetailReport([FromBody] List<int> visitIds)
{
    if (visitIds == null || !visitIds.Any())
    {
        return Ok(new List<VisitDetailReportDto>());
    }

    var visits = await _context.Visits
        .AsNoTracking()
        .Include(v => v.SalesExecutive)
        .Where(v => visitIds.Contains(v.Id))
        .ToListAsync();

    if (!visits.Any())
    {
        return NotFound(new { message = "None of the provided visit IDs were found." });
    }

    var results = new List<VisitDetailReportDto>();

    // Efficiently fetch all related data in bulk
    var locationIds = visits.Select(v => v.LocationId).Distinct().ToList();
    var schools = await _context.Schools.AsNoTracking().Where(s => locationIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
    var coachings = await _context.CoachingCenters.AsNoTracking().Where(c => locationIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
    var shops = await _context.Shopkeepers.AsNoTracking().Where(s => locationIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

    var allDistributions = await _context.BookDistributions.AsNoTracking().Include(d => d.Book).Include(d => d.Teacher).Where(d => visitIds.Contains(d.VisitId)).ToListAsync();
    var allOrders = await _context.Orders.AsNoTracking().Include(o => o.Book).Include(o => o.Teacher).Where(o => visitIds.Contains(o.VisitId)).ToListAsync();

    foreach (var visit in visits)
    {
        // Logic from single visit details, reused here
        string locationName = "Unknown";
        string? contactPersonLabel = null, contactPersonName = null, contactPersonMobile = null;

        switch (visit.LocationType)
        {
            case LocationType.School:
                if (schools.TryGetValue(visit.LocationId, out var school)) { locationName = school.Name; contactPersonLabel = "Principal"; contactPersonName = school.PrincipalName; contactPersonMobile = school.PrincipalMobileNumber; }
                break;
            case LocationType.CoachingCenter:
                if (coachings.TryGetValue(visit.LocationId, out var coaching)) { locationName = coaching.Name; contactPersonLabel = "Teacher/Contact"; contactPersonName = coaching.TeacherName; contactPersonMobile = coaching.MobileNumber; }
                break;
            case LocationType.Shopkeeper:
                if (shops.TryGetValue(visit.LocationId, out var shop)) { locationName = shop.Name; contactPersonLabel = "Owner/Shopkeeper"; contactPersonName = shop.OwnerName; contactPersonMobile = shop.MobileNumber; }
                break;
        }

        var visitDistributions = allDistributions.Where(d => d.VisitId == visit.Id).ToList();
        var visitOrders = allOrders.Where(o => o.VisitId == visit.Id).ToList();
        var teacherIdsInVisit = visitDistributions.Select(d => d.TeacherId).Union(visitOrders.Select(o => o.TeacherId)).Distinct();

        var teacherInteractions = new List<TeacherInteractionDto>();
        foreach (var teacherId in teacherIdsInVisit)
        {
            var teacher = visitDistributions.FirstOrDefault(d => d.TeacherId == teacherId)?.Teacher ?? visitOrders.FirstOrDefault(o => o.TeacherId == teacherId)?.Teacher;
            if (teacher != null)
            {
                teacherInteractions.Add(new TeacherInteractionDto
                {
                    TeacherName = teacher.Name,
                    PrimarySubject = teacher.PrimarySubject,
                    ClassesTaught = teacher.ClassesTaught,
                    WhatsAppNumber = teacher.WhatsAppNumber,
                    DistributedBooks = visitDistributions.Where(d => d.TeacherId == teacherId).Select(d => new BookDistributionDetailDto { BookTitle = d.Book.Title, Quantity = d.Quantity, WasRecommended = d.WasRecommended }).ToList(),
                    PlacedOrders = visitOrders.Where(o => o.TeacherId == teacherId).Select(o => new OrderDetailDto { BookTitle = o.Book.Title, Quantity = o.Quantity }).ToList()
                });
            }
        }

        results.Add(new VisitDetailReportDto
        {
            VisitId = visit.Id,
            ExecutiveName = visit.SalesExecutive.Name,
            VisitTimestamp = TimeZoneHelper.ConvertUtcToIst(visit.CheckInTimestamp),
            LocationName = locationName,
            LocationType = visit.LocationType.ToString(),
  ContactPersonLabel = contactPersonLabel, 
            ContactPersonName = contactPersonName,
            ContactPersonMobile = contactPersonMobile,
            PrincipalRemarks = visit.PrincipalRemarks,
            TeacherInteractions = teacherInteractions
        });
    }

    return Ok(results);
}

    // GET: /api/reports/my-visit-history?startDate=...&endDate=...&pageNumber=1&pageSize=20
    [HttpGet("my-visit-history")]
    [Authorize(Roles = "Executive")]
    public async Task<IActionResult> GetMyVisitHistory(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var executiveId = CurrentUserId;

        // Default to last 30 days if no dates provided
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var query = _context.Visits
            .Include(v => v.SalesExecutive)
            .Where(v => v.SalesExecutiveId == executiveId &&
                       v.CheckInTimestamp.Date >= start.Date &&
                       v.CheckInTimestamp.Date <= end.Date)
            .OrderByDescending(v => v.CheckInTimestamp);

        // Get total count
        var totalCount = await query.CountAsync();

        // Get paginated results
        var visits = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.CheckInTimestamp,
                v.CheckOutTimestamp,
                v.LocationId,
                v.LocationType,
                v.Status,
                v.PrincipalRemarks,
                v.PermissionToMeetTeachers,
                v.CheckInPhotoUrl,
                v.Latitude,
                v.Longitude
            })
            .ToListAsync();

        // Get location names
        var locationIds = visits.Select(v => v.LocationId).Distinct().ToList();
        var schools = await _context.Schools.Where(s => locationIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        var coachings = await _context.CoachingCenters.Where(c => locationIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
        var shops = await _context.Shopkeepers.Where(s => locationIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

        // Get counts for distributions and orders
        var visitIds = visits.Select(v => v.Id).ToList();
        var distributionCounts = await _context.BookDistributions
            .Where(d => visitIds.Contains(d.VisitId))
            .GroupBy(d => d.VisitId)
            .Select(g => new { VisitId = g.Key, Count = g.Sum(d => d.Quantity) })
            .ToDictionaryAsync(x => x.VisitId, x => x.Count);

        var orderCounts = await _context.Orders
            .Where(o => visitIds.Contains(o.VisitId))
            .GroupBy(o => o.VisitId)
            .Select(g => new { VisitId = g.Key, Count = g.Sum(o => o.Quantity) })
            .ToDictionaryAsync(x => x.VisitId, x => x.Count);

        var teacherCounts = await _context.BookDistributions
            .Where(d => visitIds.Contains(d.VisitId))
            .GroupBy(d => new { d.VisitId, d.TeacherId })
            .Select(g => new { g.Key.VisitId, g.Key.TeacherId })
            .GroupBy(x => x.VisitId)
            .Select(g => new { VisitId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.VisitId, x => x.Count);

        var result = visits.Select(v => new
        {
            visitId = v.Id,
            date = TimeZoneHelper.ConvertUtcToIst(v.CheckInTimestamp),
            checkInTime = TimeZoneHelper.ConvertUtcToIst(v.CheckInTimestamp).ToString("hh:mm tt"),
            checkOutTime = v.CheckOutTimestamp.HasValue
                ? TimeZoneHelper.ConvertUtcToIst(v.CheckOutTimestamp.Value).ToString("hh:mm tt")
                : null,
            duration = v.CheckOutTimestamp.HasValue
                ? (double?)(v.CheckOutTimestamp.Value - v.CheckInTimestamp).TotalMinutes
                : null,
            locationName = v.LocationType switch
            {
                LocationType.School => schools.GetValueOrDefault(v.LocationId)?.Name ?? "Unknown School",
                LocationType.CoachingCenter => coachings.GetValueOrDefault(v.LocationId)?.Name ?? "Unknown Coaching",
                LocationType.Shopkeeper => shops.GetValueOrDefault(v.LocationId)?.Name ?? "Unknown Shop",
                _ => "Unknown"
            },
            locationType = v.LocationType.ToString(),
            area = v.LocationType == LocationType.School
                ? schools.GetValueOrDefault(v.LocationId)?.AssignedArea
                : null,
            district = v.LocationType == LocationType.School
                ? schools.GetValueOrDefault(v.LocationId)?.City
                : (v.LocationType == LocationType.CoachingCenter
                    ? coachings.GetValueOrDefault(v.LocationId)?.City
                    : null),
            status = v.Status.ToString(),
            teachersInteracted = teacherCounts.GetValueOrDefault(v.Id, 0),
            booksDistributed = distributionCounts.GetValueOrDefault(v.Id, 0),
            ordersPlaced = orderCounts.GetValueOrDefault(v.Id, 0),
            permissionGranted = v.PermissionToMeetTeachers,
            principalRemarks = v.PrincipalRemarks,
            checkInPhotoUrl = v.CheckInPhotoUrl,
            latitude = v.Latitude,
            longitude = v.Longitude
        }).ToList();

        return Ok(new
        {
            visits = result,
            pageNumber,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

}