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

        foreach (var location in planDto.Locations)
        {
            int finalLocationId;

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
                                            Address = "Address not specified", // Provide a valid default

                           // Address = location.Address ?? "Not Provided",
                            City = location.City ?? "Indore",
                                            Pincode = "000000", // Provide a valid default

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
                            Address = location.Address ?? "Not Provided",
                            City = location.City ?? "Indore",
                            Pincode = location.Pincode ?? "452001",
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
                            Address = location.Address ?? "Not Provided",
                            City = location.City ?? "Indore",
                            Pincode = location.Pincode ?? "000000",
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