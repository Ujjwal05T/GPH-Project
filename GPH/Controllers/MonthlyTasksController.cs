using GPH.Data;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;

namespace GPH.Controllers;

[Authorize]
[ApiController]
[Route("api/monthly-tasks")]
public class MonthlyTasksController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public MonthlyTasksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Endpoint for the Admin's "Simple Task List" upload
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    // 1. ADD 'locationType' as a parameter from the form
    public async Task<IActionResult> BulkUploadTasks([FromForm] IFormFile file, [FromForm] int salesExecutiveId, [FromForm] string assignedMonth, [FromForm] int locationType)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded." });
        
        // 2. ADD validation for the locationType
        if (!Enum.IsDefined(typeof(LocationType), locationType))
        {
            return BadRequest(new { message = "Invalid location type specified." });
        }

        if (!DateTime.TryParseExact(assignedMonth, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate))
        {
            return BadRequest(new { message = "Invalid month format. Please use YYYY-MM." });
        }
        var firstDayOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);

        var newTasks = new List<MonthlyTask>();

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0);

            // 3. UPDATE column indexes to match the client's Excel format
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                string locationName = row.GetCell(1)?.ToString()?.Trim() ?? ""; // Column B is "School Name"
                if (string.IsNullOrWhiteSpace(locationName)) continue;

                newTasks.Add(new MonthlyTask
                {
                    SalesExecutiveId = salesExecutiveId,
                    AssignedMonth = firstDayOfMonth,
                    LocationName = locationName,
                    Area = row.GetCell(2)?.ToString()?.Trim(),     // Column C is "Area"
                    District = row.GetCell(3)?.ToString()?.Trim(), // Column D is "District"
                    Address = row.GetCell(4)?.ToString()?.Trim(),  // Column E is "Address"
                    IsCompleted = false,
                    LocationType = (LocationType)locationType // 4. ASSIGN the locationType
                });
            }
        }

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // 5. UPDATE the query to also filter by locationType when deleting old tasks
                var oldTasks = await _context.MonthlyTasks
                    .Where(t => t.SalesExecutiveId == salesExecutiveId && t.AssignedMonth == firstDayOfMonth && t.LocationType == (LocationType)locationType)
                    .ToListAsync();
                
                if(oldTasks.Any()) _context.MonthlyTasks.RemoveRange(oldTasks);

                await _context.MonthlyTasks.AddRangeAsync(newTasks);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Database error. Operation rolled back." });
            }
        }

        return Ok(new { message = $"Successfully assigned {newTasks.Count} tasks." });
    }

    // Endpoint for the Executive to get their task list
    [HttpGet("my-tasks")]
    [Authorize(Roles = "Executive")]
    public async Task<IActionResult> GetMyTasks()
    {
        var today = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var tasks = await _context.MonthlyTasks
            .Where(t => t.SalesExecutiveId == CurrentUserId && t.AssignedMonth == firstDayOfMonth)
            .OrderBy(t => t.IsCompleted) // Show incomplete tasks first
            .ThenBy(t => t.LocationName)
            .Select(t => new 
            {
                t.Id,
                t.LocationName,
                t.Address,
                t.District,
                t.City,
                t.IsCompleted,
                                t.LocationType // <-- THIS IS THE FIX. THIS LINE WAS MISSING.

            })
            .ToListAsync();

        return Ok(tasks);
    }

    // Endpoint for the Executive to mark a task as complete
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Executive")]
    public async Task<IActionResult> MarkTaskComplete(int id)
    {
        var task = await _context.MonthlyTasks
            .FirstOrDefaultAsync(t => t.Id == id && t.SalesExecutiveId == CurrentUserId);

        if (task == null)
        {
            return NotFound();
        }

        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Task marked as complete." });
    }
}