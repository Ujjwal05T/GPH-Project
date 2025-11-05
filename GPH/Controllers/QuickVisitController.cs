// GPH/Controllers/QuickVisitController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using GPH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize]
[ApiController]
[Route("api/quick-visit")]
public class QuickVisitController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IDailyAllowanceService _dailyAllowanceService;

    public QuickVisitController(
        ApplicationDbContext context, 
        IWebHostEnvironment hostEnvironment, 
        IDailyAllowanceService dailyAllowanceService)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
        _dailyAllowanceService = dailyAllowanceService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuickVisit([FromForm] CreateQuickVisitDto dto)
    {
        int locationId;

        if (dto.LocationType == LocationType.CoachingCenter)
        {
            var newCoachingCenter = new CoachingCenter { Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude ,    IsLocationVerified = true,    CreatedByExecutiveId = dto.SalesExecutiveId };
            _context.CoachingCenters.Add(newCoachingCenter);
            await _context.SaveChangesAsync();
            locationId = newCoachingCenter.Id;
        }
        else if (dto.LocationType == LocationType.Shopkeeper)
        {
            var newShopkeeper = new Shopkeeper { Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude ,    IsLocationVerified = true,    CreatedByExecutiveId = dto.SalesExecutiveId };
            _context.Shopkeepers.Add(newShopkeeper);
            await _context.SaveChangesAsync();
            locationId = newShopkeeper.Id;
        }
        else
        {
            return BadRequest("Invalid location type for a quick visit.");
        }

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.CheckInPhoto.FileName;
        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await dto.CheckInPhoto.CopyToAsync(fileStream);
        }
        // --- THIS IS THE FINAL FIX ---
    // After creating the visit, we also create a corresponding BeatPlan record
    // so that it will appear on the dashboard's "Today's Visits" list.
    var newPlan = new BeatPlan
    {
        SalesExecutiveId = dto.SalesExecutiveId,
        LocationId = locationId, // The ID of the new Shopkeeper/Coaching
        LocationType = dto.LocationType,
        PlanDate = TimeZoneHelper.GetCurrentIstTime().Date,
        // Mark the plan as Completed immediately since the visit is also complete
        Status = PlanStatus.Completed 
    };
    _context.BeatPlans.Add(newPlan);
    await _context.SaveChangesAsync();
    // --- END FIX ---

        var newVisit = new Visit
        {
            SalesExecutiveId = dto.SalesExecutiveId,
            LocationId = locationId,
            LocationType = dto.LocationType,
            CheckInTimestamp = DateTime.UtcNow,
            CheckOutTimestamp = DateTime.UtcNow, // Mark as completed immediately
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CheckInPhotoUrl = Path.Combine("uploads", uniqueFileName)
        };
        _context.Visits.Add(newVisit);
        await _context.SaveChangesAsync();

        if (dto.Details != null && dto.Details.Any())
        {
            var visitDetails = dto.Details.Select(kvp => new VisitDetail
            {
                VisitId = newVisit.Id,
                Key = kvp.Key,
                Value = kvp.Value
            }).ToList();
            _context.VisitDetails.AddRange(visitDetails);
            await _context.SaveChangesAsync();
        }

        // ✅ NEW APPROACH: Use BeatPlanId directly if provided
        if (dto.BeatPlanId.HasValue)
        {
            var beatPlan = await _context.BeatPlans
                .FirstOrDefaultAsync(p => p.Id == dto.BeatPlanId.Value);

            if (beatPlan != null)
            {
                beatPlan.Status = PlanStatus.Completed;
                // Update the LocationId to point to the newly created location
                beatPlan.LocationId = locationId;
                await _context.SaveChangesAsync();
            }
        }

        var daJustAwarded = await _dailyAllowanceService.CheckAndAwardExecutiveDA(newVisit.SalesExecutiveId, newVisit.CheckInTimestamp);
        if (daJustAwarded)
        {
            await _dailyAllowanceService.CheckAndAwardAsmDA(newVisit.SalesExecutiveId, newVisit.CheckInTimestamp);
        }

        return Ok(new { 
            message = "Visit created successfully.", 
            visitId = newVisit.Id,
            daTargetCompleted = daJustAwarded 
        });
    }
}