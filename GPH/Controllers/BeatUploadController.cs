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

            // Validate header to ensure correct template is being used
            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null)
            {
                return BadRequest(new { message = "Invalid file format: No header row found." });
            }

            string expectedHeader = "School Name";
            string actualHeader = headerRow.GetCell(1)?.ToString()?.Trim() ?? "";

            if (!actualHeader.Equals(expectedHeader, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = $"Wrong template file! Expected '{expectedHeader}' in column B, but found '{actualHeader}'. Please download and use the correct School template."
                });
            }

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

            // Validate header to ensure correct template is being used
            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null)
            {
                return BadRequest(new { message = "Invalid file format: No header row found." });
            }

            string expectedHeader = "Shopkeeper Name";
            string actualHeader = headerRow.GetCell(1)?.ToString()?.Trim() ?? "";

            if (!actualHeader.Equals(expectedHeader, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = $"Wrong template file! Expected '{expectedHeader}' in column B, but found '{actualHeader}'. Please download and use the correct Shopkeeper template."
                });
            }

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

            // Validate header to ensure correct template is being used
            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null)
            {
                return BadRequest(new { message = "Invalid file format: No header row found." });
            }

            string expectedHeader = "Coaching Name";
            string actualHeader = headerRow.GetCell(1)?.ToString()?.Trim() ?? "";

            if (!actualHeader.Equals(expectedHeader, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = $"Wrong template file! Expected '{expectedHeader}' in column B, but found '{actualHeader}'. Please download and use the correct Coaching template."
                });
            }

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

    // GET: /api/beat-upload/template?locationType=0
    [HttpGet("template")]
    public IActionResult DownloadTemplate([FromQuery] int locationType = 0)
    {
        if (!Enum.IsDefined(typeof(LocationType), locationType))
        {
            return BadRequest("Invalid location type specified.");
        }

        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet("Beat Assignment");

        // Create header style
        IFont boldFont = workbook.CreateFont();
        boldFont.IsBold = true;
        ICellStyle headerStyle = workbook.CreateCellStyle();
        headerStyle.SetFont(boldFont);

        // Create header row
        IRow headerRow = sheet.CreateRow(0);
        headerRow.CreateCell(0).SetCellValue("S.No");

        // Set column headers based on location type
        switch ((LocationType)locationType)
        {
            case LocationType.School:
                headerRow.CreateCell(1).SetCellValue("School Name");
                break;
            case LocationType.CoachingCenter:
                headerRow.CreateCell(1).SetCellValue("Coaching Name");
                break;
            case LocationType.Shopkeeper:
                headerRow.CreateCell(1).SetCellValue("Shopkeeper Name");
                break;
        }

        headerRow.CreateCell(2).SetCellValue("Area");
        headerRow.CreateCell(3).SetCellValue("District");
        headerRow.CreateCell(4).SetCellValue("Address");

        // Apply header style
        for (int i = 0; i < 5; i++)
        {
            headerRow.GetCell(i).CellStyle = headerStyle;
        }

        // Add sample row
        IRow sampleRow = sheet.CreateRow(1);
        sampleRow.CreateCell(0).SetCellValue(1);

        switch ((LocationType)locationType)
        {
            case LocationType.School:
                sampleRow.CreateCell(1).SetCellValue("Example School Name");
                break;
            case LocationType.CoachingCenter:
                sampleRow.CreateCell(1).SetCellValue("Example Coaching Center");
                break;
            case LocationType.Shopkeeper:
                sampleRow.CreateCell(1).SetCellValue("Example Shop Name");
                break;
        }

        sampleRow.CreateCell(2).SetCellValue("Central Area");
        sampleRow.CreateCell(3).SetCellValue("Bhopal");
        sampleRow.CreateCell(4).SetCellValue("123 Main Street");

        // Auto-size columns
        for (int i = 0; i < 5; i++)
        {
            sheet.AutoSizeColumn(i);
            // Add some padding
            sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 1024);
        }

        // Stream the file back to the user
        using (var memoryStream = new MemoryStream())
        {
            workbook.Write(memoryStream);
            var content = memoryStream.ToArray();

            string locationTypeName = ((LocationType)locationType).ToString();
            string fileName = $"BeatAssignment_{locationTypeName}_Template.xlsx";

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }

}