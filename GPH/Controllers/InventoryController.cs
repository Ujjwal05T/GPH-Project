// GPH/Controllers/InventoryController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GPH.Controllers;
[Authorize]
[ApiController]
[Route("api/inventory")] // Route will be "/api/inventory"
public class InventoryController : BaseApiController 
{
    private readonly ApplicationDbContext _context;
    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }
    // POST: /api/inventory/assign
    [HttpPost("assign")]
    public async Task<IActionResult> AssignInventory([FromBody] CreateInventoryAssignmentDto assignmentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var newAssignment = new InventoryAssignment
        {
            SalesExecutiveId = assignmentDto.SalesExecutiveId,
            BookId = assignmentDto.BookId,
            QuantityAssigned = assignmentDto.QuantityAssigned,
            DateAssigned = DateTime.UtcNow
        };
        _context.InventoryAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Inventory assigned successfully." });
    }
    // GET: /api/executives/{executiveId}/stock
      // GET: /api/executives/{executiveId}/stock
    [HttpGet("/api/executives/{executiveId}/stock")]
    public async Task<IActionResult> GetStockForExecutive(int executiveId)
    {
        // Step 1: Assignments ko group karein aur saari details lein
        var assignments = await _context.InventoryAssignments
            .Where(a => a.SalesExecutiveId == executiveId)
            // === YEH HAI CHANGE ===
            // Hum ab BookId, Title, Subject, ClassLevel, aur Medium par group karenge
            .GroupBy(a => new { 
                a.BookId, 
                a.Book.Title,
                a.Book.Subject,
                a.Book.ClassLevel,
                a.Book.Medium
            })
            .Select(g => new
            {
                BookId = g.Key.BookId,
                // Title ko yahan format karein
                BookTitle = $"{g.Key.Subject} - {g.Key.ClassLevel} ({g.Key.Medium ?? "N/A"})",
                TotalAssigned = g.Sum(a => a.QuantityAssigned)
            })
            .ToListAsync();
        // Step 2: Distributions ko group karein
        var distributions = await _context.BookDistributions
            .Where(d => d.Visit.SalesExecutiveId == executiveId)
            .GroupBy(d => d.BookId)
            .Select(g => new
            {
                BookId = g.Key,
                TotalDistributed = g.Sum(d => d.Quantity)
            })
            .ToDictionaryAsync(x => x.BookId, x => x.TotalDistributed);
        // Step 3: Data ko combine karein
        var stockReport = assignments.Select(a => new CalculatedStockDto
        {
            BookId = a.BookId,
            BookTitle = a.BookTitle, // Ab yeh formatted title hai
            TotalAssigned = a.TotalAssigned,
            TotalDistributed = distributions.GetValueOrDefault(a.BookId, 0),
            RemainingStock = a.TotalAssigned - distributions.GetValueOrDefault(a.BookId, 0)
        }).ToList();
        return Ok(stockReport);
    }
     [HttpGet("my-stock")]
    [Authorize]
    public async Task<IActionResult> GetMyStock()
    {
        var executiveId = CurrentUserId;
        // Logic bilkul GetStockForExecutive jaisa hi hai
        var assignments = await _context.InventoryAssignments
            .Where(a => a.SalesExecutiveId == executiveId)
            // === YEH HAI CHANGE ===
            .GroupBy(a => new {
                a.BookId,
                a.Book.Title,
                a.Book.Subject,
                a.Book.ClassLevel,
                a.Book.Medium
            })
            .Select(g => new
            {
                BookId = g.Key.BookId,
                BookTitle = $"{g.Key.Subject} - {g.Key.ClassLevel} ({g.Key.Medium ?? "N/A"})",
                TotalAssigned = g.Sum(a => a.QuantityAssigned)
            })
            .ToListAsync();
        var distributions = await _context.BookDistributions
            .Include(d => d.Visit)
            .Where(d => d.Visit.SalesExecutiveId == executiveId)
            .GroupBy(d => d.BookId)
            .Select(g => new { BookId = g.Key, TotalDistributed = g.Sum(d => d.Quantity) })
            .ToDictionaryAsync(x => x.BookId, x => x.TotalDistributed);
            
        var stockReport = assignments.Select(a => new
        {
            BookId = a.BookId,
            BookTitle = a.BookTitle,
            TotalAssigned = a.TotalAssigned,
            TotalDistributed = distributions.GetValueOrDefault(a.BookId, 0),
            RemainingStock = a.TotalAssigned - distributions.GetValueOrDefault(a.BookId, 0)
        })
        .Where(s => s.RemainingStock > 0)
        .ToList();
        return Ok(stockReport);
    }
}