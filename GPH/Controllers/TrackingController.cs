// GPH/Controllers/TrackingController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers; // For our TimeZoneHelper
using GPH.Models;
using GPH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // 1. Import the caching namespace

namespace GPH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : BaseApiController
{
    private readonly ApplicationDbContext _context;
      private readonly IGeocodingService _geocodingService; // 2. Inject Geocoding Service
    private readonly IMemoryCache _cache; // 3. Inject Memory Cache
        private const double GEOFENCE_FOR_ADDRESS_UPDATE_KM = 0.1; // 100 meters


    // --- CONFIGURATION ---
    // These values should eventually be moved to appsettings.json
    private const decimal RatePerKm = 2.0m; // Example TA rate
    private const decimal DailyAllowanceAmount = 300.0m; // Example DA amount
    private const int MinVisitsForDA = 4; // Minimum school visits for DA

    public TrackingController(ApplicationDbContext context,IGeocodingService geocodingService, IMemoryCache cache)

    {
        _context = context;
            _geocodingService = geocodingService;
        _cache = cache;
    }
/*
    // POST: /api/tracking/start
    [HttpPost("start")]
    public async Task<IActionResult> StartDay([FromBody] StartDayDto startDayDto)
    {
        try
        {
            var today = TimeZoneHelper.GetCurrentIstTime().Date;
            var tomorrow = today.AddDays(1);

            // More robust check for an existing session
            var existingTracking = await _context.DailyTrackings
                .FirstOrDefaultAsync(t =>
                    t.SalesExecutiveId == startDayDto.SalesExecutiveId &&
                    t.StartTime >= today && t.StartTime < tomorrow);

            if (existingTracking != null)
            {
                // If it already exists but was ended, maybe we allow a restart? For now, let's just return it.
                if (existingTracking.EndTime != null)
                {
                    return BadRequest(new { message = "Workday has already been completed for today." });
                }
                // If it exists and is active, just return the existing ID
                return Ok(new { message = "Workday was already started.", trackingId = existingTracking.Id });
            }

            var newTracking = new DailyTracking
            {
                SalesExecutiveId = startDayDto.SalesExecutiveId,
                StartTime = DateTime.UtcNow
            };

            newTracking.LocationPoints.Add(new LocationPoint
            {
                Latitude = startDayDto.Latitude,
                Longitude = startDayDto.Longitude,
                Timestamp = newTracking.StartTime
            });

            _context.DailyTrackings.Add(newTracking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Workday started successfully.", trackingId = newTracking.Id });
        }
        catch (Exception ex)
        {
            // This will catch any unexpected database errors and return a clear message
            return StatusCode(500, new { message = "An internal server error occurred.", details = ex.Message });
        }
    }
*/

// In GPH/Controllers/TrackingController.cs

[HttpPost("start")]
public async Task<IActionResult> StartDay([FromBody] StartDayDto startDayDto)
{
    try
    {
        var today = TimeZoneHelper.GetCurrentIstTime().Date;
        var tomorrow = today.AddDays(1);

        var existingTracking = await _context.DailyTrackings
            .FirstOrDefaultAsync(t => 
                t.SalesExecutiveId == startDayDto.SalesExecutiveId && 
                t.StartTime >= today && t.StartTime < tomorrow);

        if (existingTracking != null)
        {
            // --- THIS IS THE NEW LOGIC ---
            // If the day was already ended, we will "resume" it by clearing the EndTime.
            if (existingTracking.EndTime != null)
            {
                existingTracking.EndTime = null; // Re-open the tracking session
                await _context.SaveChangesAsync();
                return Ok(new { message = "Workday resumed successfully.", trackingId = existingTracking.Id });
            }
            // If it exists and is already active, just return the existing ID
            return Ok(new { message = "Workday was already active.", trackingId = existingTracking.Id });
        }

        // If no record exists for today, create a new one.
        var newTracking = new DailyTracking
        {
            SalesExecutiveId = startDayDto.SalesExecutiveId,
            StartTime = DateTime.UtcNow
        };
        newTracking.LocationPoints.Add(new LocationPoint
        {
            Latitude = startDayDto.Latitude,
            Longitude = startDayDto.Longitude,
            Timestamp = newTracking.StartTime
        });
        _context.DailyTrackings.Add(newTracking);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Workday started successfully.", trackingId = newTracking.Id });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An internal server error occurred.", details = ex.Message });
    }
}
    // POST: /api/tracking/{trackingId}/location
    [HttpPost("{trackingId}/location")]
    public async Task<IActionResult> AddLocationPoint(int trackingId, [FromBody] LocationPointDto locationDto)
    {
        var newPoint = new LocationPoint
        {
            DailyTrackingId = trackingId,
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            Timestamp = DateTime.UtcNow
        };

        _context.LocationPoints.Add(newPoint);
        await _context.SaveChangesAsync();
 // Step 2: Check if we need to update the address (this is the core logic).
        // We run this in the background so it doesn't slow down the executive's ping.
        _ = Task.Run(async () =>
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var trackingRecord = await dbContext.DailyTrackings
                .Include(t => t.LocationPoints.OrderByDescending(p => p.Timestamp).Take(2))
                .FirstOrDefaultAsync(t => t.Id == trackingId);

            if (trackingRecord == null || trackingRecord.LocationPoints.Count < 2) return;

            var currentPoint = trackingRecord.LocationPoints.First();
            var previousPoint = trackingRecord.LocationPoints.Last();

            var distance = CalculateDistance(currentPoint, previousPoint);

            // If the user has moved more than our threshold (100m)
            if (distance > GEOFENCE_FOR_ADDRESS_UPDATE_KM)
            {
                // Invalidate the cache for this user. This forces the Admin's next refresh
                // to fetch a new address from Google.
                var cacheKey = $"address:user_{trackingRecord.SalesExecutiveId}";
                _cache.Remove(cacheKey);
            }
        });
        return Ok();
    }

    [HttpPost("{trackingId}/end")]
    public async Task<IActionResult> EndDay(int trackingId)
    {
        var trackingRecord = await _context.DailyTrackings
            .Include(t => t.LocationPoints)
            .Include(t => t.SalesExecutive) // Executive ko include karna zaroori hai
            .FirstOrDefaultAsync(t => t.Id == trackingId);
        if (trackingRecord == null)
        {
            return NotFound(new { message = "Tracking session not found." });
        }
        // EndTime set karna zaroori hai, taaki "Day Started" button dobara dikhe
        trackingRecord.EndTime = DateTime.UtcNow;
        // --- Calculate Total Distance ---
        decimal totalDistance = 0;
        var points = trackingRecord.LocationPoints.OrderBy(p => p.Timestamp).ToList();
        if (points.Count > 1)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                totalDistance += (decimal)CalculateDistance(points[i], points[i + 1]);
            }
        }
        trackingRecord.TotalDistanceKm = totalDistance;
        // --- YEH HAI POORA FIX ---
        var today = TimeZoneHelper.GetCurrentIstTime().Date;
        var executive = trackingRecord.SalesExecutive;
        // --- 1. TA (Travel Allowance) ko Handle Karein ---
        var newTaAmount = totalDistance * executive.TaRatePerKm;

        var existingTaExpense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.SalesExecutiveId == trackingRecord.SalesExecutiveId &&
                                      e.Type == ExpenseType.TravelAllowance &&
                                      e.ExpenseDate.Date == today);
        if (existingTaExpense != null)
        {
            // Agar pehle se hai, toh bas Amount aur Description update karein
            existingTaExpense.Amount = newTaAmount;
            existingTaExpense.Description = $"UPDATED: Automated TA for {totalDistance:F2} km traveled at a rate of {executive.TaRatePerKm}/km.";
        }
        else
        {
            // Agar nahi hai, toh naya banayein
            var taExpense = new Expense
            {
                SalesExecutiveId = trackingRecord.SalesExecutiveId,
                Type = ExpenseType.TravelAllowance,
                Amount = newTaAmount,
                ExpenseDate = TimeZoneHelper.GetCurrentIstTime(),
                Description = $"Automated TA for {totalDistance:F2} km traveled at a rate of {executive.TaRatePerKm}/km.",
                Status = ApprovalStatus.Pending // Hamesha Pending se shuru hoga
            };
            _context.Expenses.Add(taExpense);
        }
        // --- 2. DA (Daily Allowance) ko Handle Karein ---
        var daAlreadyExists = await _context.Expenses
            .AnyAsync(e => e.SalesExecutiveId == trackingRecord.SalesExecutiveId &&
                           e.Type == ExpenseType.DailyAllowance &&
                           e.ExpenseDate.Date == today);
        // Sirf tabhi DA check karein jab pehle se na mila ho
        if (!daAlreadyExists)
        {
            var todaysVisits = await _context.Visits
                .Where(v => v.SalesExecutiveId == trackingRecord.SalesExecutiveId && v.CheckInTimestamp.Date == today)
                .ToListAsync();
            int schoolVisits = todaysVisits.Count(v => v.LocationType == LocationType.School);
            int coachingVisits = todaysVisits.Count(v => v.LocationType == LocationType.CoachingCenter);
            int shopkeeperVisits = todaysVisits.Count(v => v.LocationType == LocationType.Shopkeeper);
            // MinVisitsForDA ko 4 maan rahe hain
            if (schoolVisits >= 4 && coachingVisits >= 1 && shopkeeperVisits >= 1)
            {
                var daExpense = new Expense
                {
                    SalesExecutiveId = trackingRecord.SalesExecutiveId,
                    Type = ExpenseType.DailyAllowance,
                    Amount = executive.DaAmount, // Executive ke profile se DA amount lein
                    ExpenseDate = TimeZoneHelper.GetCurrentIstTime(),
                    Description = $"Automated DA for completing {schoolVisits} schools, {coachingVisits} coaching, and {shopkeeperVisits} shopkeepers.",
                    Status = ApprovalStatus.Pending
                };
                _context.Expenses.Add(daExpense);
            }
        }

        // --- END FIX ---
        await _context.SaveChangesAsync();
        return Ok(new
        {
            message = "Workday ended. Expenses have been generated or updated.",
            totalDistance = trackingRecord.TotalDistanceKm
        });
    }

    // POST: /api/tracking/{trackingId}/end
    // In GPH/Controllers/TrackingController.cs

    // ... (constructor and other methods like StartDay, AddLocationPoint)

    // [HttpPost("{trackingId}/end")]
    // public async Task<IActionResult> EndDay(int trackingId)
    // {
    //     var trackingRecord = await _context.DailyTrackings
    //         .Include(t => t.LocationPoints)
    //                 .Include(t => t.SalesExecutive) // This Include is now essential
    //  // Include location points for distance calculation
    //         .FirstOrDefaultAsync(t => t.Id == trackingId);

    //     if (trackingRecord == null)
    //     {
    //         return NotFound(new { message = "Tracking session not found." });
    //     }

    //     if (trackingRecord.EndTime.HasValue)
    //     {
    //         return BadRequest(new { message = "Workday has already been ended." });
    //     }

    //     trackingRecord.EndTime = DateTime.UtcNow;

    //     // --- Calculate Total Distance ---
    //     decimal totalDistance = 0;
    //     var points = trackingRecord.LocationPoints.OrderBy(p => p.Timestamp).ToList();
    //     if (points.Count > 1) // Need at least two points to calculate distance
    //     {
    //         for (int i = 0; i < points.Count - 1; i++)
    //         {
    //             totalDistance += (decimal)CalculateDistance(points[i], points[i + 1]);
    //         }
    //     }
    //     trackingRecord.TotalDistanceKm = totalDistance;

    //         // --- Create TA Expense ---
    //         var executive = trackingRecord.SalesExecutive;

    //     var taExpense = new Expense
    //     {
    //         SalesExecutiveId = trackingRecord.SalesExecutiveId,
    //         Type = ExpenseType.TravelAllowance,
    //         Amount = totalDistance * executive.TaRatePerKm,
    //         ExpenseDate = TimeZoneHelper.GetCurrentIstTime(),
    //         Description = $"Automated TA for {totalDistance:F2} km traveled at a rate of {executive.TaRatePerKm}/km."
    //     };
    //     _context.Expenses.Add(taExpense);

    //     // --- REVISED "Check for DA" LOGIC ---
    //     var today = TimeZoneHelper.GetCurrentIstTime().Date;

    //     // Get all of today's visits for this executive, including the type of location visited
    //     var todaysVisits = await _context.Visits
    //         .Where(v => v.SalesExecutiveId == trackingRecord.SalesExecutiveId && v.CheckInTimestamp.Date == today)
    //         .ToListAsync();

    //     // Count the visits by type
    //     int schoolVisits = todaysVisits.Count(v => v.LocationType == LocationType.School);
    // int coachingVisits = todaysVisits.Count(v => v.LocationType == LocationType.CoachingCenter);
    // int shopkeeperVisits = todaysVisits.Count(v => v.LocationType == LocationType.Shopkeeper);

    //     // Assuming MinVisitsForDA is now the school minimum, e.g., 4
    //     if (schoolVisits >= MinVisitsForDA && coachingVisits >= 1 && shopkeeperVisits >= 1)
    //     {
    //         var daExpense = new Expense
    //         {
    //             SalesExecutiveId = trackingRecord.SalesExecutiveId,
    //             Type = ExpenseType.DailyAllowance,
    //             Amount = DailyAllowanceAmount,
    //             ExpenseDate = TimeZoneHelper.GetCurrentIstTime(),
    //             Description = $"Automated DA for completing {schoolVisits} schools, {coachingVisits} coaching, and {shopkeeperVisits} shopkeepers."
    //         };
    //         _context.Expenses.Add(daExpense);
    //     }
    //     // --- END REVISED LOGIC ---

    //     await _context.SaveChangesAsync();

    // return Ok(new { 
    //         message = "Workday ended. Expenses have been generated.",
    //         totalDistance = trackingRecord.TotalDistanceKm 
    //     });}

    // ... (The CalculateDistance helper method remains the same)
    // GET: /api/tracking/live
    // In GPH/Controllers/TrackingController.cs

    // In GPH/Controllers/TrackingController.cs

    // [HttpGet("live")]
    // public async Task<IActionResult> GetLiveLocations()
    // {
    //     var today = TimeZoneHelper.GetCurrentIstTime().Date;
    //     var tomorrow = today.AddDays(1);

    //     // --- STEP 1: Build the query ---
    //     var query = _context.DailyTrackings
    //         .Include(dt => dt.SalesExecutive)
    //             .ThenInclude(e => e.Manager) // Include Manager for ASM name
    //         .Where(dt =>
    //             dt.StartTime >= today &&
    //             dt.StartTime < tomorrow &&
    //             dt.EndTime == null &&
    //             dt.SalesExecutive.Status == UserStatus.Active);

    //     // --- STEP 2: Apply the ASM filter to the query ---
    //     if (CurrentUserRole == "ASM")
    //     {
    //         query = query.Where(dt => dt.SalesExecutive.ManagerId == CurrentUserId);
    //     }

    //     // --- STEP 3: Execute the query we just built ---
    //     var activeTrackingSessions = await query.ToListAsync();

    //     // --- STEP 4: Loop through the results (this part is correct) ---
    //     var latestLocations = new List<LiveLocationDto>();
    //     foreach (var session in activeTrackingSessions)
    //     {
    //         var latestPoint = await _context.LocationPoints
    //             .Where(lp => lp.DailyTrackingId == session.Id)
    //             .OrderByDescending(lp => lp.Timestamp)
    //             .FirstOrDefaultAsync();

    //         if (latestPoint != null)
    //         {
    //             latestLocations.Add(new LiveLocationDto
    //             {
    //                 SalesExecutiveId = session.SalesExecutiveId,
    //                 ExecutiveName = session.SalesExecutive.Name,
    //                 AsmName = session.SalesExecutive.Manager?.Name ?? "N/A",
    //                 Latitude = latestPoint.Latitude,
    //                 Longitude = latestPoint.Longitude,
    //                 LastUpdated = DateTime.SpecifyKind(latestPoint.Timestamp, DateTimeKind.Utc)
    //             });
    //         }
    //     }

    //     return Ok(latestLocations);
    // }
    [HttpGet("live")]
    public async Task<IActionResult> GetLiveLocations()
    {
        var today = TimeZoneHelper.GetCurrentIstTime().Date;
        var tomorrow = today.AddDays(1);

        var query = _context.DailyTrackings
            .Include(dt => dt.SalesExecutive).ThenInclude(e => e.Manager)
            .Where(dt => dt.StartTime >= today && dt.StartTime < tomorrow && dt.EndTime == null && dt.SalesExecutive.Status == UserStatus.Active);

        if (CurrentUserRole == "ASM")
        {
            query = query.Where(dt => dt.SalesExecutive.ManagerId == CurrentUserId);
        }

        var activeTrackingSessions = await query
            .Select(dt => new {
                dt.SalesExecutiveId,
                dt.SalesExecutive.Name,
                ManagerName = dt.SalesExecutive.Manager != null ? dt.SalesExecutive.Manager.Name : "N/A",
                LatestPoint = dt.LocationPoints.OrderByDescending(lp => lp.Timestamp).FirstOrDefault()
            })
            .Where(x => x.LatestPoint != null)
            .ToListAsync();

        var liveData = new List<LiveLocationWithAddressDto>();

        foreach (var session in activeTrackingSessions)
        {
            var cacheKey = $"address:user_{session.SalesExecutiveId}";
            
            // Try to get the address from the cache first.
            if (!_cache.TryGetValue(cacheKey, out string? cachedAddress))
            {
                // CACHE MISS: Address not found in cache, so call Google API
                var geocodingResult = await _geocodingService.GetCoordinatesAsync($"{session.LatestPoint!.Latitude},{session.LatestPoint!.Longitude}");
                cachedAddress = geocodingResult?.Address ?? "Location not found"; // Use the new Address property

                // Store the result in the cache for 5 minutes
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                
                _cache.Set(cacheKey, cachedAddress, cacheEntryOptions);
            }

            liveData.Add(new LiveLocationWithAddressDto
            {
                SalesExecutiveId = session.SalesExecutiveId,
                ExecutiveName = session.Name,
                AsmName = session.ManagerName,
                Latitude = session.LatestPoint!.Latitude,
                Longitude = session.LatestPoint!.Longitude,
                LastUpdated = DateTime.SpecifyKind(session.LatestPoint.Timestamp, DateTimeKind.Utc),
                Address = cachedAddress ?? "Resolving address..." // Use the address from cache or the fresh one
            });
        }

        return Ok(liveData);
    }

    [HttpGet("status")]
[Authorize(Roles = "Executive")]
public async Task<IActionResult> GetTodaysTrackingStatus()
{
    var today = TimeZoneHelper.GetCurrentIstTime().Date;
    var trackingRecord = await _context.DailyTrackings
        .Where(t => t.SalesExecutiveId == CurrentUserId && t.StartTime.Date == today && t.EndTime == null)
        .FirstOrDefaultAsync();

    if (trackingRecord != null)
    {
        // A session is active
        return Ok(new { isActive = true, trackingId = trackingRecord.Id });
    }
    
    // No active session
    return Ok(new { isActive = false, trackingId = (int?)null });
}

    [HttpGet("history")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> GetTrackingHistory([FromQuery] int executiveId, [FromQuery] DateTime date)
    {
        // Security check for ASMs
        if (CurrentUserRole == "ASM")
        {
            var isMyExecutive = await _context.SalesExecutives.AnyAsync(e => e.Id == executiveId && e.ManagerId == CurrentUserId);
            if (!isMyExecutive) return Forbid();
        }

        // Use EF.Functions for a reliable, database-side date comparison
        var trackingRecord = await _context.DailyTrackings
            .Include(dt => dt.LocationPoints)
            .Where(dt => dt.SalesExecutiveId == executiveId &&
                         EF.Functions.DateDiffDay(dt.StartTime, date) == 0)
            .FirstOrDefaultAsync();

        if (trackingRecord == null)
        {
            return NotFound(new { message = "No tracking data found for this executive on the selected date." });
        }

        // --- Fetch the Beat Plan for that day to show as markers ---
        var dayStart = date.Date;
        var plans = await _context.BeatPlans
            .Where(p => p.SalesExecutiveId == executiveId && p.PlanDate == dayStart)
            .ToListAsync();

        // Enrich the plan data with location names and coordinates
        var schoolIds = plans.Where(p => p.LocationType == LocationType.School).Select(p => p.LocationId).ToList();
        var coachingIds = plans.Where(p => p.LocationType == LocationType.CoachingCenter).Select(p => p.LocationId).ToList();
        var shopkeeperIds = plans.Where(p => p.LocationType == LocationType.Shopkeeper).Select(p => p.LocationId).ToList();

        var schools = await _context.Schools.Where(s => schoolIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        var coachings = await _context.CoachingCenters.Where(c => coachingIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
        var shopkeepers = await _context.Shopkeepers.Where(s => shopkeeperIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

        var plannedVisitsDto = plans.Select(p =>
        {
            double? lat = null;
            double? lon = null;
            string name = "Unknown";

            switch (p.LocationType)
            {
                case LocationType.School:
                    if (schools.TryGetValue(p.LocationId, out var school)) { name = school.Name; lat = school.OfficialLatitude; lon = school.OfficialLongitude; }
                    break;
                case LocationType.CoachingCenter:
                    if (coachings.TryGetValue(p.LocationId, out var coaching)) { name = coaching.Name; lat = coaching.Latitude; lon = coaching.Longitude; }
                    break;
                case LocationType.Shopkeeper:
                    if (shopkeepers.TryGetValue(p.LocationId, out var shopkeeper)) { name = shopkeeper.Name; lat = shopkeeper.Latitude; lon = shopkeeper.Longitude; }
                    break;
            }

            return new BeatPlanDto
            {
                Id = p.Id,
                LocationId = p.LocationId,
                LocationName = name,
                Latitude = lat,
                Longitude = lon,
                // You can add other BeatPlanDto properties here if needed
            };
        }).ToList();
        // --- End Plan Fetching ---

        var replayData = new RouteReplayDto
        {
            TrackingId = trackingRecord.Id,
            StartTime = trackingRecord.StartTime,
            EndTime = trackingRecord.EndTime,
            TotalDistanceKm = trackingRecord.TotalDistanceKm,
            Path = trackingRecord.LocationPoints
                    .OrderBy(p => p.Timestamp)
                    .Select(p => new LocationPointDto
                    {
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        Timestamp = p.Timestamp
                    }).ToList(),
            PlannedVisits = plannedVisitsDto
        };

        return Ok(replayData);
    }
    [HttpGet("summary")]
[Authorize(Roles = "Executive")]
public async Task<IActionResult> GetDaySummary([FromQuery] DateTime date)
{
    var dayStart = date.Date;
    var trackingRecord = await _context.DailyTrackings
        .Where(dt => dt.SalesExecutiveId == CurrentUserId && 
                     EF.Functions.DateDiffDay(dt.StartTime, dayStart) == 0)
        .FirstOrDefaultAsync();

    if (trackingRecord == null)
    {
        return Ok(new { totalDistanceKm = 0 }); // Return 0 if no tracking for that day
    }

    return Ok(new { totalDistanceKm = trackingRecord.TotalDistanceKm });
}


    

    // Helper method to calculate distance between two GPS points (Haversine formula)
    private double CalculateDistance(LocationPoint p1, LocationPoint p2)
    {
        var d1 = p1.Latitude * (Math.PI / 180.0);
        var num1 = p1.Longitude * (Math.PI / 180.0);
        var d2 = p2.Latitude * (Math.PI / 180.0);
        var num2 = p2.Longitude * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        return 6371 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }

}