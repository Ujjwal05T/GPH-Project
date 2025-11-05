// GPH/Controllers/ShopkeepersController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ShopkeepersController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public ShopkeepersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/shopkeepers
    [HttpGet]
    public async Task<IActionResult> GetAllShopkeepers()
    {
        var shopkeepers = await _context.Shopkeepers
            .Select(s => new ShopkeeperDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                City = s.City
            })
            .ToListAsync();
        return Ok(shopkeepers);
    }

    // POST: /api/shopkeepers
    [HttpPost]
    public async Task<IActionResult> CreateShopkeeper([FromBody] CreateShopkeeperDto dto)
    {
        var newShopkeeper = new Shopkeeper
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        _context.Shopkeepers.Add(newShopkeeper);
        await _context.SaveChangesAsync();

        var resultDto = new ShopkeeperDto
        {
            Id = newShopkeeper.Id,
            Name = newShopkeeper.Name,
            Address = newShopkeeper.Address,
            City = newShopkeeper.City
        };

        return CreatedAtAction(nameof(GetShopkeeperById), new { id = resultDto.Id }, resultDto);
    }

    // GET: /api/shopkeepers/{id} - Helper endpoint
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShopkeeperById(int id)
    {
        var shopkeeper = await _context.Shopkeepers.FindAsync(id);
        if (shopkeeper == null) return NotFound();
        return Ok(shopkeeper);
    }
}