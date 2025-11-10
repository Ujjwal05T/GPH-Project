// GPH/Controllers/BeatPlansController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GPH.Controllers;

[Authorize]
[ApiController]
[Route("api/beatplans")] // <-- CHANGE 1: The route is now specific and unambiguous
public class BeatPlansController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public BeatPlansController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: /api/beatplans

    [HttpPost]
    public async Task<IActionResult> CreateBeatPlan([FromBody] CreateBeatPlanDto planDto)
    {
          if (!ModelState.IsValid) return BadRequest(ModelState);
                if (planDto.SalesExecutiveId != CurrentUserId && CurrentUserRole != "Admin") return Forbid();
                var planDate = planDto.PlanDate.ToUniversalTime().Date;
                var planIsForToday = planDto.PlanDate.Date == TimeZoneHelper.GetCurrentIstTime().Date;
                var initialStatus = planIsForToday ? PlanStatus.Approved : PlanStatus.PendingApproval;
                var existingPlannedLocationIds = await _context.BeatPlans
               .Where(p => p.SalesExecutiveId == planDto.SalesExecutiveId && p.PlanDate == planDate)
               .Select(p => p.LocationId)
               .ToListAsync();
        var newPlans = new List<BeatPlan>();
             const double proximityThresholdKm = 0.1; // 100 meters


         using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
              




                foreach (var location in planDto.Locations)
                {
                    int finalLocationId;
                    LocationType finalLocationType = location.LocationType;


                    if (location.LocationId.HasValue && location.LocationId > 0)
                    {
                        // Existing location
                        finalLocationId = location.LocationId.Value;
                    }
                    else
                    {
                        // Creating a new location
                        if (string.IsNullOrEmpty(location.NewLocationName) ||
                            !location.Latitude.HasValue ||
                            !location.Longitude.HasValue)
                        {
                            // Skip invalid entry
                            continue;
                        }
                        int? foundLocationId = null;
                        switch (location.LocationType)
                        {
                            case LocationType.School:
                                var nearbySchools = await _context.Schools
                                    .Where(s => s.OfficialLatitude.HasValue && s.OfficialLongitude.HasValue)
                                    .ToListAsync();
                                var closestSchool = nearbySchools
                                    .Select(s => new { s.Id, Distance = GeoHelper.GetDistance(location.Latitude.Value, location.Longitude.Value, s.OfficialLatitude, s.OfficialLongitude) })
                                    .Where(s => s.Distance < proximityThresholdKm)
                                    .OrderBy(s => s.Distance).FirstOrDefault();
                                if (closestSchool != null) foundLocationId = closestSchool.Id;
                                break;
                            
                            case LocationType.CoachingCenter:
                                var nearbyCoachings = await _context.CoachingCenters
                                    .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
                                    .ToListAsync();
                                var closestCoaching = nearbyCoachings
                                    .Select(c => new { c.Id, Distance = GeoHelper.GetDistance(location.Latitude.Value, location.Longitude.Value, c.Latitude, c.Longitude) })
                                    .Where(c => c.Distance < proximityThresholdKm)
                                    .OrderBy(c => c.Distance).FirstOrDefault();
                                if (closestCoaching != null) foundLocationId = closestCoaching.Id;
                                break;

                            case LocationType.Shopkeeper:
                                var nearbyShops = await _context.Shopkeepers
                                    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
                                    .ToListAsync();
                                var closestShop = nearbyShops
                                    .Select(s => new { s.Id, Distance = GeoHelper.GetDistance(location.Latitude.Value, location.Longitude.Value, s.Latitude, s.Longitude) })
                                    .Where(s => s.Distance < proximityThresholdKm)
                                    .OrderBy(s => s.Distance).FirstOrDefault();
                                if (closestShop != null) foundLocationId = closestShop.Id;
                                break;
                        }
                        // --- END OF MISSING BLOCK ---

                        if (foundLocationId.HasValue)
                        {
                            // A duplicate was found! Use its ID.
                            finalLocationId = foundLocationId.Value;
                        }
                        else // No duplicate found, so create a new one.
                        {




                            switch (location.LocationType)
                            {
                                case LocationType.School:
                                    var newSchool = new School
                                    {
                                        Name = location.NewLocationName.Length > 200
                                            ? location.NewLocationName.Substring(0, 200)
                                            : location.NewLocationName,
                                        OfficialLatitude = location.Latitude.Value,
                                        OfficialLongitude = location.Longitude.Value,
                                        // Address = "Address not specified", // Provide a valid default
                                        Address = location.Address ?? "Address not specified",


                                        // Address = location.Address ?? "Not Provided",
                                        City = location.City ?? "Unknown",
                                        // Pincode = "000000", // Provide a valid default
                                        Pincode = location.Pincode ?? "000000",


                                        //   Pincode = location.Pincode ?? "452001",
                                        PrincipalName = "N/A",
                                        TotalStudentCount = 0,
                                        AssignedArea = "Uncategorized",
                                        CreatedByExecutiveId = planDto.SalesExecutiveId

                                    };
                                    _context.Schools.Add(newSchool);
                                    await _context.SaveChangesAsync();
                                    finalLocationId = newSchool.Id;
                                    break;

                                case LocationType.CoachingCenter:
                                    var newCenter = new CoachingCenter
                                    {
                                        Name = location.NewLocationName,
                                        Latitude = location.Latitude.Value,
                                        Longitude = location.Longitude.Value,
                                        Address = location.Address,
                                        City = location.City,
                                        Pincode = location.Pincode,
                                        CreatedByExecutiveId = planDto.SalesExecutiveId

                                    };
                                    _context.CoachingCenters.Add(newCenter);
                                    await _context.SaveChangesAsync();
                                    finalLocationId = newCenter.Id;
                                    break;

                                case LocationType.Shopkeeper:
                                    var newShop = new Shopkeeper
                                    {
                                        Name = location.NewLocationName,
                                        Latitude = location.Latitude.Value,
                                        Longitude = location.Longitude.Value,
                                        Address = location.Address,
                                        City = location.City,
                                        Pincode = location.Pincode,
                                        CreatedByExecutiveId = planDto.SalesExecutiveId

                                    };
                                    _context.Shopkeepers.Add(newShop);
                                    await _context.SaveChangesAsync();
                                    finalLocationId = newShop.Id;
                                    break;

                                default:
                                    return BadRequest("Unsupported location type.");
                            }
                        }
                        
                    }
                    if (existingPlannedLocationIds.Contains(finalLocationId))
                    {
                        continue; // Skip this iteration, it's a duplicate
                    }

                    // Add the beat plan
                    newPlans.Add(new BeatPlan
                    {
                        SalesExecutiveId = planDto.SalesExecutiveId,
                        LocationId = finalLocationId,
                        LocationType = location.LocationType,
                        PlanDate = planDto.PlanDate.ToUniversalTime().Date,
                        Status = initialStatus
                    });
                }

                if (newPlans.Any())
                {
                    _context.BeatPlans.AddRange(newPlans);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
                  catch (Exception ex)
            {
                // --- ADD THIS BLOCK ---
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred. The plan was not saved.", details = ex.Message });
            }
        }

return Ok(new { message = $"{newPlans.Count} new planned visits were added. Duplicates were ignored." });
    }


    // GET: /api/beatplans/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBeatPlanById(int id)
    {
        var plan = await _context.BeatPlans.FindAsync(id);
        if (plan == null) return NotFound();

        object? locationDetails = null;
        switch (plan.LocationType)
        {
            case LocationType.School:
                locationDetails = await _context.Schools.FindAsync(plan.LocationId);
                break;
            case LocationType.CoachingCenter:
                locationDetails = await _context.CoachingCenters.FindAsync(plan.LocationId);
                break;
            case LocationType.Shopkeeper:
                locationDetails = await _context.Shopkeepers.FindAsync(plan.LocationId);
                break;
        }

        if (locationDetails == null)
        {
            return NotFound("The location for this plan could not be found.");
        }

        return Ok(locationDetails);
    }
    // PATCH: /api/beatplans/approve?executiveId=13&planDate=2025-09-30
    [HttpPost("beatplans/approve")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> ApproveBeatPlan([FromQuery] int executiveId, [FromQuery] DateTime planDate)
    {
        var today = planDate.Date;


        var plansToApprove = await _context.BeatPlans
            .Where(p => p.SalesExecutiveId == executiveId &&
                         p.PlanDate == today &&
                         p.Status == PlanStatus.PendingApproval)
            .ToListAsync();

        if (!plansToApprove.Any())
        {
            return NotFound(new { message = "No pending plans found for this user on the selected date." });
        }

        foreach (var plan in plansToApprove)
        {
            plan.Status = PlanStatus.Approved;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Plan for {today:yyyy-MM-dd} has been approved." });
    }


    // --- CHANGE 4: The GetBeatPlanForExecutive method has been MOVED ---
    // It no longer exists in this file.
}