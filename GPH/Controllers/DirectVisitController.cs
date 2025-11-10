// GPH/Controllers/DirectVisitController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using GPH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- ADD THIS LINE
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
        // Use a transaction for data safety. All operations must succeed or none will.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                int finalLocationId;
                var locationType = (LocationType)dto.LocationType;
                const double proximityThresholdKm = 0.2; // 200 meters
                // --- START: "FIND OR CREATE" LOGIC ---
                int? foundLocationId = null;
                switch (locationType)
                {
                    case LocationType.School:
                        var potentialSchools = await _context.Schools
                            .Where(s => s.City == dto.City && s.District == dto.District)
                            .ToListAsync();
                        var closestSchool = potentialSchools
                            .Select(s => new { s.Id, Distance = GeoHelper.GetDistance(dto.Latitude, dto.Longitude, s.OfficialLatitude, s.OfficialLongitude) })
                            .Where(s => s.Distance < proximityThresholdKm)
                            .OrderBy(s => s.Distance).FirstOrDefault();
                        if (closestSchool != null) foundLocationId = closestSchool.Id;
                        break;
                    case LocationType.CoachingCenter:
                        var potentialCoachings = await _context.CoachingCenters
                            .Where(c => c.City == dto.City && c.District == dto.District)
                            .ToListAsync();
                        var closestCoaching = potentialCoachings
                            .Select(c => new { c.Id, Distance = GeoHelper.GetDistance(dto.Latitude, dto.Longitude, c.Latitude, c.Longitude) })
                            .Where(c => c.Distance < proximityThresholdKm)
                            .OrderBy(c => c.Distance).FirstOrDefault();
                        if (closestCoaching != null) foundLocationId = closestCoaching.Id;
                        break;
                    case LocationType.Shopkeeper:
                        var potentialShops = await _context.Shopkeepers
                            .Where(s => s.City == dto.City && s.District == dto.District)
                            .ToListAsync();
                        var closestShop = potentialShops
                            .Select(s => new { s.Id, Distance = GeoHelper.GetDistance(dto.Latitude, dto.Longitude, s.Latitude, s.Longitude) })
                            .Where(s => s.Distance < proximityThresholdKm)
                            .OrderBy(s => s.Distance).FirstOrDefault();
                        if (closestShop != null) foundLocationId = closestShop.Id;
                        break;
                }
                if (foundLocationId.HasValue)
                {
                    finalLocationId = foundLocationId.Value;
                }
                else // No nearby duplicate found, so create a new one
                {
                    switch (locationType)
                    {
                        case LocationType.School:
                            var newSchool = new School { 
                                Name = dto.LocationName, OfficialLatitude = dto.Latitude, OfficialLongitude = dto.Longitude, 
                                Address = dto.Address ?? "N/A", City = dto.City ?? "N/A", District = dto.District, Pincode = dto.Pincode ?? "N/A",
                                IsLocationVerified = true, CreatedByExecutiveId = dto.SalesExecutiveId, CreatedAt = DateTime.UtcNow, PrincipalName = "N/A"
                            };
                            _context.Schools.Add(newSchool);
                            await _context.SaveChangesAsync(); // Save to get the new ID
                            finalLocationId = newSchool.Id;
                            break;
                        case LocationType.CoachingCenter:
                            var newCoaching = new CoachingCenter { 
                                Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude, 
                                Address = dto.Address, City = dto.City, District = dto.District, Pincode = dto.Pincode,
                                IsLocationVerified = true, CreatedByExecutiveId = dto.SalesExecutiveId, CreatedAt = DateTime.UtcNow
                            };
                            _context.CoachingCenters.Add(newCoaching);
                            await _context.SaveChangesAsync();
                            finalLocationId = newCoaching.Id;
                            break;
                        case LocationType.Shopkeeper:
                            var newShop = new Shopkeeper { 
                                Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude, 
                                Address = dto.Address, City = dto.City, District = dto.District, Pincode = dto.Pincode,
                                IsLocationVerified = true, CreatedByExecutiveId = dto.SalesExecutiveId, CreatedAt = DateTime.UtcNow
                            };
                            _context.Shopkeepers.Add(newShop);
                            await _context.SaveChangesAsync();
                            finalLocationId = newShop.Id;
                            break;
                        default:
                            return BadRequest("Invalid location type.");
                    }
                }
                // --- END: "FIND OR CREATE" LOGIC ---
                // 2. Save the photo
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.Photo.FileName;
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(fileStream);
                }
                // 3. Create a completed Visit record
                var newVisit = new Visit
                {
                    SalesExecutiveId = dto.SalesExecutiveId,
                    LocationId = finalLocationId,
                    LocationType = locationType,
                    CheckInTimestamp = DateTime.UtcNow,
                    CheckOutTimestamp = DateTime.UtcNow,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    CheckInPhotoUrl = Path.Combine("uploads", uniqueFileName),
                    Status = VisitStatus.Completed
                };
                _context.Visits.Add(newVisit);
                // 4. Create a completed BeatPlan record
                var newPlan = new BeatPlan
                {
                    SalesExecutiveId = dto.SalesExecutiveId,
                    LocationId = finalLocationId,
                    LocationType = locationType,
                    PlanDate = DateTime.UtcNow.Date,
                    Status = PlanStatus.Completed 
                };
                _context.BeatPlans.Add(newPlan);
                
                // Save Visit and BeatPlan together
                await _context.SaveChangesAsync();
                // Commit the transaction now that all database operations are successful
                await transaction.CommitAsync();
                // 5. Check for DA (can be done after the transaction)
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
            catch (Exception ex)
            {
                // If anything fails, roll back all database changes
                await transaction.RollbackAsync();
                // Log the detailed error on the server for debugging
                Console.WriteLine($"ERROR in CreateDirectVisit: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while saving the visit. The operation was rolled back." });
            }
        }
    }
}