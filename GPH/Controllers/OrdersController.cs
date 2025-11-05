// GPH/Controllers/OrdersController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers; // Add the namespace declaration

[Authorize] // All methods require a login by default
[ApiController] // Add ApiController attribute for standard API behaviors
[Route("api/[controller]")]
public class OrdersController : BaseApiController // Inherit from BaseApiController
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: /api/orders
    [HttpPost]
    [Authorize(Roles = "Executive")] // Only Executives can create orders
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var visitExists = await _context.Visits.AnyAsync(v => v.Id == dto.VisitId);
    if (!visitExists)
    {
        return BadRequest(new { message = $"Invalid VisitId: {dto.VisitId}" });
    }

    // 2. Check if the Teacher exists
    var teacherExists = await _context.Teachers.AnyAsync(t => t.Id == dto.TeacherId);
    if (!teacherExists)
    {
        return BadRequest(new { message = $"Invalid TeacherId: {dto.TeacherId}" });
    }

    // 3. Check if all BookIds exist
    var bookIdsInOrder = dto.Items.Select(i => i.BookId).ToList();
    var existingBooksCount = await _context.Books.CountAsync(b => bookIdsInOrder.Contains(b.Id));
    if (existingBooksCount != bookIdsInOrder.Count)
    {
        return BadRequest(new { message = "One or more BookIds in the order are invalid." });
    }

        var newOrders = dto.Items.Select(item => new Order
        {
            VisitId = dto.VisitId,
            TeacherId = dto.TeacherId,
            BookId = item.BookId,
            Quantity = item.Quantity,
            UnitPrice = 0, // Placeholder - should be fetched from the Book entity
            OrderDate = DateTime.UtcNow
        }).ToList();

        _context.Orders.AddRange(newOrders);
        await _context.SaveChangesAsync();
        
        // In Phase 2, we would trigger the WhatsApp message here.
        return Ok(new { message = "Order placed successfully." });
    }
    // GET: /api/orders

   [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Book)
            .Include(o => o.Teacher)
            .Include(o => o.Visit).ThenInclude(v => v.SalesExecutive)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        // --- THIS IS THE FIX: Enrich the data after fetching ---
        var locationIds = orders.Select(o => o.Visit.LocationId).Distinct().ToList();
        var schools = await _context.Schools.Where(s => locationIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
        // Add similar fetches for CoachingCenters and Shopkeepers if orders can be placed there

        var resultDtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            BookTitle = o.Book.Title,
            BookSubject = o.Book.Subject,
            BookClassLevel = o.Book.ClassLevel,
            Quantity = o.Quantity,
            TeacherName = o.Teacher.Name,
            SchoolName = o.Visit.LocationType == LocationType.School ? schools.GetValueOrDefault(o.Visit.LocationId)?.Name ?? "N/A" : "N/A",
            SchoolArea = o.Visit.LocationType == LocationType.School ? schools.GetValueOrDefault(o.Visit.LocationId)?.AssignedArea ?? "N/A" : "N/A",
            ExecutiveName = o.Visit.SalesExecutive.Name
        }).ToList();

        return Ok(resultDtos);
    }

    // GET: /api/orders/my-orders
       [HttpGet("my-orders")]
    [Authorize(Roles = "Executive")]
    public async Task<IActionResult> GetMyOrders()
    {
        var executiveId = CurrentUserId;
        var orders = await _context.Orders
            .Include(o => o.Book)
            .Include(o => o.Teacher)
            .Include(o => o.Visit)
            .Where(o => o.Visit.SalesExecutiveId == executiveId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        // --- REPEAT THE SAME FIX HERE ---
        var locationIds = orders.Select(o => o.Visit.LocationId).Distinct().ToList();
        var schools = await _context.Schools.Where(s => locationIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

        var resultDtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            BookTitle = o.Book.Title,
            BookSubject = o.Book.Subject,
            BookClassLevel = o.Book.ClassLevel,
            Quantity = o.Quantity,
            TeacherName = o.Teacher.Name,
            SchoolName = o.Visit.LocationType == LocationType.School ? schools.GetValueOrDefault(o.Visit.LocationId)?.Name ?? "N/A" : "N/A",
            SchoolArea = o.Visit.LocationType == LocationType.School ? schools.GetValueOrDefault(o.Visit.LocationId)?.AssignedArea ?? "N/A" : "N/A",
            ExecutiveName = "" // Not needed here
        }).ToList();

        return Ok(resultDtos);
    }
}