// GPH/Controllers/DirectVisitController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using GPH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GPH.Controllers;
[Authorize(Roles = "Executive")]
[ApiController]
[Route("api/direct-visit")]
public class DirectVisitController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IDailyAllowanceService _dailyAllowanceService;
    public DirectVisitController(
        ApplicationDbContext context, 
        IWebHostEnvironment hostEnvironment, 
        IDailyAllowanceService dailyAllowanceService)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
        _dailyAllowanceService = dailyAllowanceService;
    }
    [HttpPost]
    public async Task<IActionResult> CreateDirectVisit([FromForm] CreateDirectVisitDto dto)
    {
        int locationId;
        var locationType = (LocationType)dto.LocationType;
        // 1. Location ko create karein
        switch (locationType)
        {
            case LocationType.School:
                var newSchool = new School { Name = dto.LocationName, OfficialLatitude = dto.Latitude, OfficialLongitude = dto.Longitude, IsLocationVerified = true, CreatedByExecutiveId = dto.SalesExecutiveId, Address = "N/A", City = "N/A", Pincode = "N/A", PrincipalName = "N/A" };
                _context.Schools.Add(newSchool);
                await _context.SaveChangesAsync();
                locationId = newSchool.Id;
                break;
            case LocationType.CoachingCenter:
                var newCoaching = new CoachingCenter { Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude, IsLocationVerified = true, CreatedByExecutiveId = dto.SalesExecutiveId };
                _context.CoachingCenters.Add(newCoaching);
                await _context.SaveChangesAsync();
                locationId = newCoaching.Id;
                break;
            case LocationType.Shopkeeper:
                var newShop = new Shopkeeper { Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude, IsLocationVerified = true, CreatedByExecutiveId = dto.SalesExecutiveId };
                _context.Shopkeepers.Add(newShop);
                await _context.SaveChangesAsync();
                locationId = newShop.Id;
                break;
            default:
                return BadRequest("Invalid location type.");
        }
        // 2. Photo ko save karein
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.Photo.FileName;
        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await dto.Photo.CopyToAsync(fileStream);
        }
        // 3. Ek completed Visit record banayein
        var newVisit = new Visit
        {
            SalesExecutiveId = dto.SalesExecutiveId,
            LocationId = locationId,
            LocationType = locationType,
            CheckInTimestamp = DateTime.UtcNow,
            CheckOutTimestamp = DateTime.UtcNow, // Instantly complete
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CheckInPhotoUrl = Path.Combine("uploads", uniqueFileName),
            Status = VisitStatus.Completed
        };
        _context.Visits.Add(newVisit);
        // 4. Ek completed BeatPlan record banayein (taaki dashboard par dikhe)
        var newPlan = new BeatPlan
        {
            SalesExecutiveId = dto.SalesExecutiveId,
            LocationId = locationId,
            LocationType = locationType,
PlanDate = DateTime.UtcNow.Date,
            Status = PlanStatus.Completed 
        };
        _context.BeatPlans.Add(newPlan);
        await _context.SaveChangesAsync();
        // 5. DA ke liye check karein
        var daJustAwarded = await _dailyAllowanceService.CheckAndAwardExecutiveDA(newVisit.SalesExecutiveId, newVisit.CheckInTimestamp);
        if (daJustAwarded)
        {
            await _dailyAllowanceService.CheckAndAwardAsmDA(newVisit.SalesExecutiveId, newVisit.CheckInTimestamp);
        }
        return Ok(new { 
            message = "Direct Visit created successfully.", 
            visitId = newVisit.Id,
            daTargetCompleted = daJustAwarded 
        });
    }
}