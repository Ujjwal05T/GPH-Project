// GPH/Controllers/VisitsController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize]
[ApiController]

[Route("api/visits")] 
public class VisitsController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDailyAllowanceService _dailyAllowanceService; // <-- ADD THIS LINE

    private const double GeofenceRadiusKm = 1.0; // 1 km

    public VisitsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, IDailyAllowanceService dailyAllowanceService) // <-- ADD THE SERVICE TO THE CONSTRUCTOR
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
            _dailyAllowanceService = dailyAllowanceService; // <-- ADD THIS LINE

    }

    [HttpPost("checkin")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CheckIn([FromForm] CreateVisitDto visitDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
         var today = TimeZoneHelper.GetCurrentIstTime().Date;
    var existingVisit = await _context.Visits
        .FirstOrDefaultAsync(v => 
            v.SalesExecutiveId == visitDto.SalesExecutiveId &&
            v.LocationId == visitDto.LocationId &&
            v.LocationType == visitDto.LocationType &&
            EF.Functions.DateDiffDay(v.CheckInTimestamp, today) == 0 &&
            v.Status == VisitStatus.InProgress);

    if (existingVisit != null)
    {
        // A visit is already in progress. Don't create a new one.
        // Just return the existing visit's data so the frontend can resume.
        return Ok(new { Visit = new { Id = existingVisit.Id }, DaTargetCompleted = false });
    }

        // --- GEOFENCING VALIDATION (Only for Schools) ---
        if (visitDto.LocationType == LocationType.School)
        {
            var school = await _context.Schools.FindAsync(visitDto.LocationId);
            if (school == null)
            {
                return NotFound(new { message = "School not found." });
            }
            if (!school.IsLocationVerified)
{
    // This is the first verified visit. Update the school's coordinates.
    school.OfficialLatitude = visitDto.Latitude;
    school.OfficialLongitude = visitDto.Longitude;
    school.IsLocationVerified = true;
    // No need to save yet, SaveChangesAsync at the end will handle it.
}

            if (school.OfficialLatitude.HasValue && school.OfficialLongitude.HasValue)
            {
                var distance = CalculateDistance(
                    school.OfficialLatitude.Value, school.OfficialLongitude.Value,
                    visitDto.Latitude, visitDto.Longitude
                );

                if (distance > GeofenceRadiusKm)
                {
                    return BadRequest(new { message = $"Check-in failed. You are {distance * 1000:F0} meters away from the school." });
                }
            }
        }
        // --- END GEOFENCING ---

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + visitDto.CheckInPhoto.FileName;
        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await visitDto.CheckInPhoto.CopyToAsync(fileStream);
        }

        var newVisit = new Visit
        {
            SalesExecutiveId = visitDto.SalesExecutiveId,
            LocationId = visitDto.LocationId,
            LocationType = visitDto.LocationType,
            CheckInTimestamp = DateTime.UtcNow,
            Latitude = visitDto.Latitude,
            Longitude = visitDto.Longitude,
            Notes = visitDto.Notes,
            CheckInPhotoUrl = Path.Combine("uploads", uniqueFileName),
            Status = VisitStatus.InProgress

        };

        _context.Visits.Add(newVisit);
        await _context.SaveChangesAsync();
        var daJustAwarded = await _dailyAllowanceService.CheckAndAwardExecutiveDA(newVisit.SalesExecutiveId, newVisit.CheckInTimestamp);
if (daJustAwarded)
{
    await _dailyAllowanceService.CheckAndAwardAsmDA(newVisit.SalesExecutiveId, newVisit.CheckInTimestamp);
}
        
        return Ok(new { 
    Visit = new { Id = newVisit.Id }, // Send a simplified object
    DaTargetCompleted = daJustAwarded 
});

       
    }

    [HttpGet]
    public async Task<IActionResult> GetAllVisits()
    {
        // This query is complex because we need to get the name from three different tables.
        // We will build the result in memory after fetching the raw visit data.
        var visits = await _context.Visits.OrderByDescending(v => v.CheckInTimestamp).ToListAsync();
        var schoolIds = visits.Where(v => v.LocationType == LocationType.School).Select(v => v.LocationId).ToList();
        var coachingIds = visits.Where(v => v.LocationType == LocationType.CoachingCenter).Select(v => v.LocationId).ToList();
        var shopkeeperIds = visits.Where(v => v.LocationType == LocationType.Shopkeeper).Select(v => v.LocationId).ToList();

        var schools = await _context.Schools.Where(s => schoolIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);
        var coachings = await _context.CoachingCenters.Where(c => coachingIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);
        var shopkeepers = await _context.Shopkeepers.Where(s => shopkeeperIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);

        var resultDtos = visits.Select(v => new VisitDto
        {
            Id = v.Id,
            SalesExecutiveId = v.SalesExecutiveId,
            LocationId = v.LocationId,
            LocationType = v.LocationType,
            LocationName = v.LocationType switch
            {
                LocationType.School => schools.GetValueOrDefault(v.LocationId, "Unknown School"),
                LocationType.CoachingCenter => coachings.GetValueOrDefault(v.LocationId, "Unknown Coaching"),
                LocationType.Shopkeeper => shopkeepers.GetValueOrDefault(v.LocationId, "Unknown Shopkeeper"),
                _ => "Unknown"
            },
            CheckInTimestamp = v.CheckInTimestamp,
            CheckInTimestampIst = TimeZoneHelper.ConvertUtcToIst(v.CheckInTimestamp),
            CheckInPhotoUrl = v.CheckInPhotoUrl,
            Latitude = v.Latitude,
            Longitude = v.Longitude,
            Notes = v.Notes
        }).ToList();

        return Ok(resultDtos);
    }

    [HttpPost("{visitId}/details")]
    public async Task<IActionResult> AddVisitDetails(int visitId, [FromBody] Dictionary<string, string> details)
    {
        var visitExists = await _context.Visits.AnyAsync(v => v.Id == visitId);
        if (!visitExists) return NotFound("Visit not found.");

        var visitDetails = details.Select(kvp => new VisitDetail
        {
            VisitId = visitId,
            Key = kvp.Key,
            Value = kvp.Value
        }).ToList();

        _context.VisitDetails.AddRange(visitDetails);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Visit details saved successfully." });
    }

   // [HttpPut("{id}/principal-meeting")]
   [HttpPost("{id}/principal-meeting")]
    public async Task<IActionResult> UpdatePrincipalMeeting(int id, [FromBody] PrincipalMeetingDto meetingDto)
    {
        var visit = await _context.Visits.FindAsync(id);
        if (visit == null) return NotFound();

        visit.PrincipalRemarks = meetingDto.PrincipalRemarks;
        visit.PermissionToMeetTeachers = meetingDto.PermissionToMeetTeachers;

        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpPost("{id}/end")]
public async Task<IActionResult> EndVisit(int id)
{
    var visit = await _context.Visits.FindAsync(id);
    if (visit == null)
    {
        return NotFound();
    }

    // Security check can be added here to ensure the exec owns this visit

    visit.CheckOutTimestamp = DateTime.UtcNow;
    visit.Status = VisitStatus.Completed; // Set the status to Completed

    var today = TimeZoneHelper.GetCurrentIstTime().Date;
    var correspondingPlan = await _context.BeatPlans
        .FirstOrDefaultAsync(p => 
            p.SalesExecutiveId == visit.SalesExecutiveId &&
            p.LocationId == visit.LocationId &&
            p.LocationType == visit.LocationType &&
            p.PlanDate == today &&
                        p.Status != PlanStatus.Completed); // Find the first uncompleted plan for this location today


    if (correspondingPlan != null)
        {
            correspondingPlan.Status = PlanStatus.Completed; // Mark the plan as completed
        }
    await _context.SaveChangesAsync();

    return Ok(new { message = "Visit ended successfully." });
}

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371;
        var dLat = (lat2 - lat1) * (Math.PI / 180.0);
        var dLon = (lon2 - lon1) * (Math.PI / 180.0);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * (Math.PI / 180.0)) * Math.Cos(lat2 * (Math.PI / 180.0)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}