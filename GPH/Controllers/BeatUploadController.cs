// GPH/Controllers/BeatUploadController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using GPH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace GPH.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/beat-upload")]
public class BeatUploadController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IGeocodingService _geocodingService;

    public BeatUploadController(ApplicationDbContext context, IGeocodingService geocodingService)
    {
        _context = context;
        _geocodingService = geocodingService;
    }

    [HttpPost("schools")]
    public async Task<IActionResult> BulkUploadSchoolBeat([FromForm] BulkBeatAssignmentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var firstDayOfMonth = new DateTime(dto.AssignedMonth.Year, dto.AssignedMonth.Month, 1);
        var newAssignments = new List<BeatAssignment>();
        var errors = new List<string>();

        using (var stream = new MemoryStream())
        {
            await dto.File.CopyToAsync(stream);
            stream.Position = 0;
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                string schoolName = row.GetCell(1)?.ToString()?.Trim() ?? "";
                string area = row.GetCell(2)?.ToString()?.Trim() ?? "";
                string district = row.GetCell(3)?.ToString()?.Trim() ?? "";
                string address = row.GetCell(4)?.ToString()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(schoolName)) continue;

                var existingSchool = await _context.Schools
    .FirstOrDefaultAsync(s => s.Name.ToLower() == schoolName.ToLower());

                int locationId;
                if (existingSchool != null)
                {
                    locationId = existingSchool.Id;
                }
                else
                {
                    // --- INTELLIGENT GEOCODING LOGIC ---
                    string fullSearchQuery = $"{schoolName}, {address}, {district}, Madhya Pradesh,India";
                    var geocodingResult = await _geocodingService.GetCoordinatesAsync(fullSearchQuery);

                    if (geocodingResult == null)
                    {
                        errors.Add($"Row {i + 1}: Could not find a valid GPS location for '{schoolName}'. The location was skipped.");
                        continue; // Skip this row
                    }

                    var newSchool = new School
                    {
                        Name = schoolName,
                        Address = address ?? "N/A",
                        City = district ?? "Unknown",
                        AssignedArea = area ?? "Uncategorized",
                        Pincode = "000000",
                        PrincipalName = "N/A",
                        TotalStudentCount = 0,
                        OfficialLatitude = geocodingResult.Latitude,
                        OfficialLongitude = geocodingResult.Longitude
                    };
                    _context.Schools.Add(newSchool);
                    await _context.SaveChangesAsync();
                    locationId = newSchool.Id;
                }

                newAssignments.Add(new BeatAssignment
                {
                    SalesExecutiveId = dto.SalesExecutiveId,
                    AssignedMonth = firstDayOfMonth,
                    LocationName = schoolName,
                    Area = area,
                    District = district,
                    Address = address,
                    LocationId = locationId, // The ID we found or created
    LocationType = LocationType.School // Assuming the upload is for schools
                });
            }
        }

        // Clear old assignments for this user and month
        var oldAssignments = await _context.BeatAssignments
            .Where(a => a.SalesExecutiveId == dto.SalesExecutiveId && a.AssignedMonth == firstDayOfMonth)
            .ToListAsync();
        _context.BeatAssignments.RemoveRange(oldAssignments);

        // Add the new assignments
        await _context.BeatAssignments.AddRangeAsync(newAssignments);
        await _context.SaveChangesAsync();

        if (errors.Any())
        {
            return Ok(new
            {
                message = $"Upload partially successful. {newAssignments.Count} locations were assigned, but {errors.Count} failed.",
                errors = errors
            });
        }

        return Ok(new { message = $"Successfully processed and assigned {newAssignments.Count} locations." });
    }
    [HttpPost("preview")]
    public async Task<IActionResult> PreviewSchoolBeat([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var preview = new BeatPreviewDto();
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                string schoolName = row.GetCell(1)?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(schoolName)) continue;

                var item = new BeatPreviewItemDto { LocationName = schoolName };

                var existingSchool = await _context.Schools
                    .FirstOrDefaultAsync(s => s.Name.Equals(schoolName, StringComparison.OrdinalIgnoreCase));

                if (existingSchool != null)
                {
                    item.Status = "Exists in DB";
                    item.Details = $"Will be assigned with ID: {existingSchool.Id}";
                }
                else
                {
                    string address = row.GetCell(4)?.ToString()?.Trim() ?? "";
                    string district = row.GetCell(3)?.ToString()?.Trim() ?? "";
                    string fullSearchQuery = $"{schoolName}, {address}, {district}, India";
                    var geocodingResult = await _geocodingService.GetCoordinatesAsync(fullSearchQuery);

                    if (geocodingResult != null)
                    {
                        item.Status = "New (Found on Google)";
                        item.Details = $"Will be created at Lat: {geocodingResult.Latitude:F4}, Lon: {geocodingResult.Longitude:F4}";
                    }
                    else
                    {
                        item.Status = "Error";
                        item.Details = "Could not find this location in the database or on Google Maps.";
                    }
                }
                preview.Items.Add(item);
            }
        }
        return Ok(preview);
    }
 // POST: /api/beat-upload/shopkeepers
    [HttpPost("shopkeepers")]
    public async Task<IActionResult> BulkUploadShopkeeperBeat([FromForm] BulkBeatAssignmentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0) return BadRequest("No file uploaded.");
        var firstDayOfMonth = new DateTime(dto.AssignedMonth.Year, dto.AssignedMonth.Month, 1);
        var assignments = new List<BeatAssignment>();

        using (var stream = new MemoryStream())
        {
            await dto.File.CopyToAsync(stream);
            stream.Position = 0;
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                string shopName = row.GetCell(1)?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(shopName)) continue;

                string addressDistrict = row.GetCell(2)?.ToString()?.Trim() ?? "";
                string ownerName = row.GetCell(3)?.ToString()?.Trim() ?? "";
                string mobileNo = row.GetCell(4)?.ToString()?.Trim() ?? "";

                var existingShop = await _context.Shopkeepers.FirstOrDefaultAsync(s => s.Name.ToLower() == shopName.ToLower());
                int locationId;
                if (existingShop != null)
                {
                    locationId = existingShop.Id;
                }
                else
                {
                    var newShop = new Shopkeeper { Name = shopName, Address = addressDistrict, OwnerName = ownerName, MobileNumber = mobileNo };
                    _context.Shopkeepers.Add(newShop);
                    await _context.SaveChangesAsync();
                    locationId = newShop.Id;
                }
                assignments.Add(new BeatAssignment { SalesExecutiveId = dto.SalesExecutiveId, LocationId = locationId, LocationType = LocationType.Shopkeeper, AssignedMonth = firstDayOfMonth, LocationName = shopName, Address = addressDistrict });
            }
        }

        var oldAssignments = await _context.BeatAssignments.Where(a => a.SalesExecutiveId == dto.SalesExecutiveId && a.AssignedMonth == firstDayOfMonth && a.LocationType == LocationType.Shopkeeper).ToListAsync();
        _context.BeatAssignments.RemoveRange(oldAssignments);
        await _context.BeatAssignments.AddRangeAsync(assignments);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Successfully assigned {assignments.Count} shopkeepers." });
    }

    // POST: /api/beat-upload/coaching
    [HttpPost("coaching")]
    public async Task<IActionResult> BulkUploadCoachingBeat([FromForm] BulkBeatAssignmentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0) return BadRequest("No file uploaded.");
        var firstDayOfMonth = new DateTime(dto.AssignedMonth.Year, dto.AssignedMonth.Month, 1);
        var assignments = new List<BeatAssignment>();

        using (var stream = new MemoryStream())
        {
            await dto.File.CopyToAsync(stream);
            stream.Position = 0;
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                string coachingName = row.GetCell(2)?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(coachingName)) continue;

                string address = row.GetCell(3)?.ToString()?.Trim() ?? "";
                string district = row.GetCell(4)?.ToString()?.Trim() ?? "";
                string teacherName = row.GetCell(5)?.ToString()?.Trim() ?? "";
                string mobileNo = row.GetCell(6)?.ToString()?.Trim() ?? "";
                
                var existingCoaching = await _context.CoachingCenters.FirstOrDefaultAsync(c => c.Name.ToLower() == coachingName.ToLower());
                int locationId;
                if (existingCoaching != null)
                {
                    locationId = existingCoaching.Id;
                }
                else
                {
                    var newCoaching = new CoachingCenter { Name = coachingName, Address = address, City = district, TeacherName = teacherName, MobileNumber = mobileNo };
                    _context.CoachingCenters.Add(newCoaching);
                    await _context.SaveChangesAsync();
                    locationId = newCoaching.Id;
                }
                assignments.Add(new BeatAssignment { SalesExecutiveId = dto.SalesExecutiveId, LocationId = locationId, LocationType = LocationType.CoachingCenter, AssignedMonth = firstDayOfMonth, LocationName = coachingName, Address = address, District = district });
            }
        }
        
        var oldAssignments = await _context.BeatAssignments.Where(a => a.SalesExecutiveId == dto.SalesExecutiveId && a.AssignedMonth == firstDayOfMonth && a.LocationType == LocationType.CoachingCenter).ToListAsync();
        _context.BeatAssignments.RemoveRange(oldAssignments);
        await _context.BeatAssignments.AddRangeAsync(assignments);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Successfully assigned {assignments.Count} coaching centers." });
    }

}