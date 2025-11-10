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
         var address = Request.Form["Address"].FirstOrDefault();
        var city = Request.Form["City"].FirstOrDefault();
        var district = Request.Form["District"].FirstOrDefault();
        var pincode = Request.Form["Pincode"].FirstOrDefault();

        if (dto.LocationType == LocationType.CoachingCenter)
        {
           // var newCoachingCenter = new CoachingCenter { Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude ,    IsLocationVerified = true,    CreatedByExecutiveId = dto.SalesExecutiveId };
           var newCoachingCenter = new CoachingCenter
           {
               Name = dto.LocationName,
               Latitude = dto.Latitude,
               Longitude = dto.Longitude,
               Address = address,
               City = city,
               Pincode = pincode,
               IsLocationVerified = true,
               CreatedByExecutiveId = dto.SalesExecutiveId,
               CreatedAt = DateTime.UtcNow,
               // --- ADD THESE MISSING FIELDS ---
               TeacherName = Request.Form["Details[TeacherName]"].FirstOrDefault(),
               Subjects = Request.Form["Details[CoachingSubjects]"].FirstOrDefault(),
               Classes = Request.Form["Details[CoachingClasses]"].FirstOrDefault(),
                MobileNumber = Request.Form["Details[CoachingTeacherMobile]"].FirstOrDefault(),

               StudentCount = int.TryParse(Request.Form["Details[CoachingStrength]"].FirstOrDefault(), out var count) ? count : null
           };
            _context.CoachingCenters.Add(newCoachingCenter);
            await _context.SaveChangesAsync();
            locationId = newCoachingCenter.Id;
        }
        else if (dto.LocationType == LocationType.Shopkeeper)
        {
                    // 1. Parse the StockStatus from the form data
        var stockStatusString = Request.Form["Details[StockStatus]"].FirstOrDefault();
        Enum.TryParse<StockStatus>(stockStatusString, true, out var stockStatus); // Use TryParse for safety

        // 2. Check if this visit is for an EXISTING shopkeeper (from a beat plan)
        Shopkeeper? shopkeeperToUpdate = null;
        if (dto.BeatPlanId.HasValue)
        {
            var plan = await _context.BeatPlans.FindAsync(dto.BeatPlanId.Value);
            if (plan != null)
            {
                // Find the shopkeeper linked to this plan
                shopkeeperToUpdate = await _context.Shopkeepers.FindAsync(plan.LocationId);
            }
        }

        if (shopkeeperToUpdate != null)
        {
            // If we found an existing shopkeeper, just update their stock status
            shopkeeperToUpdate.CurrentStockStatus = stockStatus;
            _context.Shopkeepers.Update(shopkeeperToUpdate); // Mark it for update
            await _context.SaveChangesAsync();
            locationId = shopkeeperToUpdate.Id;
        }
        else
        {
            // If it's a new shopkeeper, create a new one
            var newShopkeeper = new Shopkeeper 
            { 
                Name = dto.LocationName, 
                Latitude = dto.Latitude, 
                Longitude = dto.Longitude,
                Address = address,
                City = city,
                Pincode = pincode,
                IsLocationVerified = true,
                CreatedByExecutiveId = dto.SalesExecutiveId,
                CreatedAt = DateTime.UtcNow,
                OwnerName = Request.Form["Details[ShopkeeperName]"].FirstOrDefault(),
                MobileNumber = Request.Form["Details[WhatsAppNumber]"].FirstOrDefault(),
                
                // 3. Set the CurrentStockStatus for the new shopkeeper
                CurrentStockStatus = stockStatus 
            };
            _context.Shopkeepers.Add(newShopkeeper);
            await _context.SaveChangesAsync();
            locationId = newShopkeeper.Id;
        }
        // --- END OF CHANGES FOR SHOPKEEPER ---
    }
    else
    {
        return BadRequest("Invalid location type for a quick visit.");
    }
            
        //     //var newShopkeeper = new Shopkeeper { Name = dto.LocationName, Latitude = dto.Latitude, Longitude = dto.Longitude ,    IsLocationVerified = true,    CreatedByExecutiveId = dto.SalesExecutiveId };
        //      var newShopkeeper = new Shopkeeper 
        //     { 
        //         Name = dto.LocationName, 
        //         Latitude = dto.Latitude, 
        //         Longitude = dto.Longitude,
        //         Address = address,
        //         City = city,
        //         Pincode = pincode,
        //         IsLocationVerified = true,
        //         CreatedByExecutiveId = dto.SalesExecutiveId,
        //         CreatedAt = DateTime.UtcNow,
        //         // --- ADD THESE MISSING FIELDS ---
        //         OwnerName = Request.Form["Details[ShopkeeperName]"].FirstOrDefault(),
        //         MobileNumber = Request.Form["Details[WhatsAppNumber]"].FirstOrDefault()
        //     };
        //     _context.Shopkeepers.Add(newShopkeeper);
        //     await _context.SaveChangesAsync();
        //     locationId = newShopkeeper.Id;
        // }
        // else
        // {
        //     return BadRequest("Invalid location type for a quick visit.");
        // }

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