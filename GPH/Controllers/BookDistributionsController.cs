// GPH/Controllers/BookDistributionsController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[ApiController]
[Route("api/[controller]")] // Route will be "/api/bookdistributions"
public class BookDistributionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BookDistributionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: /api/bookdistributions
    [HttpPost]
    public async Task<IActionResult> CreateDistribution([FromBody] CreateBookDistributionDto distributionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Optional: Add validation to ensure the VisitId, TeacherId, and BookId are valid
        // For now, we will assume they are correct for speed.

        var newDistribution = new BookDistribution
        {
            VisitId = distributionDto.VisitId,
            TeacherId = distributionDto.TeacherId,
            BookId = distributionDto.BookId,
            Quantity = distributionDto.Quantity,
            WasRecommended = distributionDto.WasRecommended
        };

        _context.BookDistributions.Add(newDistribution);
        await _context.SaveChangesAsync();

        var resultDto = new BookDistributionDto
        {
            Id = newDistribution.Id,
            VisitId = newDistribution.VisitId,
            TeacherId = newDistribution.TeacherId,
            BookId = newDistribution.BookId,
            Quantity = newDistribution.Quantity,
            WasRecommended = newDistribution.WasRecommended
        };

        return Ok(resultDto);
    }

    // GET: /api/visits/{visitId}/distributions
    // A useful endpoint to get all book distributions for a specific visit
    [HttpGet("/api/visits/{visitId}/distributions")]
    public async Task<IActionResult> GetDistributionsForVisit(int visitId)
    {
        var distributions = await _context.BookDistributions
            .Where(d => d.VisitId == visitId)
            .Select(d => new BookDistributionDto
            {
                Id = d.Id,
                VisitId = d.VisitId,
                TeacherId = d.TeacherId,
                BookId = d.BookId,
                Quantity = d.Quantity,
                WasRecommended = d.WasRecommended
            })
            .ToListAsync();

        return Ok(distributions);
    }
}