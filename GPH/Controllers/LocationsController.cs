// GPH/Controllers/LocationsController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[Authorize]
[ApiController]
[Route("api/locations")]
public class LocationsController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public LocationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/locations/search?term=Modern
    [HttpGet("search")]
    public async Task<IActionResult> SearchLocations([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
        {
            return Ok(new List<LocationSearchResultDto>());
        }

        var searchTerm = term.ToLower();
        var results = new List<LocationSearchResultDto>();

        // --- THIS IS THE FIX: Run queries one after another ---
        var schools = await _context.Schools
            .Where(s => s.Name.ToLower().Contains(searchTerm))
            .Select(s => new LocationSearchResultDto
            {
                Id = s.Id,
                Name = s.Name,
                Type = LocationType.School,
                Address = s.Address,
                Latitude = s.OfficialLatitude,
                Longitude = s.OfficialLongitude
            })
            .ToListAsync();
        results.AddRange(schools);

        var coachings = await _context.CoachingCenters
            .Where(c => c.Name.ToLower().Contains(searchTerm))
            .Select(c => new LocationSearchResultDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = LocationType.CoachingCenter,
                Address = c.Address,
                Latitude = c.Latitude,
                Longitude = c.Longitude
            })
            .ToListAsync();
        results.AddRange(coachings);

        var shopkeepers = await _context.Shopkeepers
            .Where(s => s.Name.ToLower().Contains(searchTerm))
            .Select(s => new LocationSearchResultDto
            {
                Id = s.Id,
                Name = s.Name,
                Type = LocationType.Shopkeeper,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude
            })
            .ToListAsync();
        results.AddRange(shopkeepers);
        // --- END FIX ---

        var finalResults = results
            .Where(r => r.Latitude.HasValue && r.Longitude.HasValue)
            .OrderBy(r => r.Name)
            .Take(20)
            .ToList();

        return Ok(finalResults);
    }
    [HttpGet("my-assigned-markers")]
    [Authorize(Roles = "Executive")]
    public async Task<IActionResult> GetMyAssignedMarkers()
    {
        var executiveId = CurrentUserId;
        var today = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        // --- THIS IS THE FIX ---
        // 1. Get the raw list of assigned location names WITHOUT converting to lowercase yet.
        var assignedLocationNames = await _context.BeatAssignments
            .Where(a => a.SalesExecutiveId == executiveId && a.AssignedMonth == firstDayOfMonth)
            .Select(a => a.LocationName)
            .ToListAsync();

        if (!assignedLocationNames.Any())
        {
            return Ok(new List<MapMarkerDto>());
        }

        // 2. Search the tables using a translatable case-insensitive comparison.
        // We will convert the DATABASE column to lowercase for the comparison.
        var lowerCaseNames = assignedLocationNames.Select(n => n.ToLower()).ToList();

        var schools = await _context.Schools
            .Where(s => lowerCaseNames.Contains(s.Name.ToLower()) && s.OfficialLatitude.HasValue)
            .Select(s => new MapMarkerDto { Id = s.Id, Name = s.Name, Type = LocationType.School, Latitude = s.OfficialLatitude!.Value, Longitude = s.OfficialLongitude!.Value })
            .ToListAsync();

        var coachings = await _context.CoachingCenters
            .Where(c => lowerCaseNames.Contains(c.Name.ToLower()) && c.Latitude.HasValue)
            .Select(c => new MapMarkerDto { Id = c.Id, Name = c.Name, Type = LocationType.CoachingCenter, Latitude = c.Latitude!.Value, Longitude = c.Longitude!.Value })
            .ToListAsync();

        var shopkeepers = await _context.Shopkeepers
            .Where(s => lowerCaseNames.Contains(s.Name.ToLower()) && s.Latitude.HasValue)
            .Select(s => new MapMarkerDto { Id = s.Id, Name = s.Name, Type = LocationType.Shopkeeper, Latitude = s.Latitude!.Value, Longitude = s.Longitude!.Value })
            .ToListAsync();
        // --- END FIX ---

        // 3. Combine the results and return them
        var allMarkers = schools.Concat(coachings).Concat(shopkeepers).ToList();
        return Ok(allMarkers);
    }
    [HttpGet("all")]
    [Authorize(Roles = "Admin")] // Only Admins need the full list
    public async Task<IActionResult> GetAllLocations()
    {
        var schools = await _context.Schools
            .Select(s => new { Id = s.Id, Name = s.Name, Type = LocationType.School })
            .ToListAsync();

        var coachings = await _context.CoachingCenters
            .Select(c => new { Id = c.Id, Name = c.Name, Type = LocationType.CoachingCenter })
            .ToListAsync();

        var shopkeepers = await _context.Shopkeepers
            .Select(s => new { Id = s.Id, Name = s.Name, Type = LocationType.Shopkeeper })
            .ToListAsync();

        var allLocations = schools
            .Concat(coachings)
            .Concat(shopkeepers)
            .OrderBy(l => l.Name)
            .ToList();

        return Ok(allLocations);
    }

[HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyLocations([FromQuery] double lat, [FromQuery] double lng)
    {
        const double searchRadiusKm = 0.2; // 200 meters
        var results = new List<LocationSearchResultDto>();
        // Search Schools
        var nearbySchools = (await _context.Schools.ToListAsync())
            .Select(s => new {
                Location = s,
                Distance = GeoHelper.GetDistance(lat, lng, s.OfficialLatitude, s.OfficialLongitude)
            })
            .Where(s => s.Distance < searchRadiusKm)
            .OrderBy(s => s.Distance)
            .Select(s => new LocationSearchResultDto {
                Id = s.Location.Id,
                Name = s.Location.Name,
                Type = LocationType.School,
                Address = s.Location.Address,
                Latitude = s.Location.OfficialLatitude,
            Longitude = s.Location.OfficialLongitude
            });
        results.AddRange(nearbySchools);
        // Search Coaching Centers
        var nearbyCoachings = (await _context.CoachingCenters.ToListAsync())
            .Select(c => new {
                Location = c,
                Distance = GeoHelper.GetDistance(lat, lng, c.Latitude, c.Longitude)
            })
            .Where(c => c.Distance < searchRadiusKm)
            .OrderBy(c => c.Distance)
            .Select(c => new LocationSearchResultDto {
                Id = c.Location.Id,
                Name = c.Location.Name,
                Type = LocationType.CoachingCenter,
                Address = c.Location.Address,
                Latitude = c.Location.Latitude,
                Longitude = c.Location.Longitude
            });
        results.AddRange(nearbyCoachings);
        // Search Shopkeepers
        var nearbyShops = (await _context.Shopkeepers.ToListAsync())
            .Select(s => new {
                Location = s,
                Distance = GeoHelper.GetDistance(lat, lng, s.Latitude, s.Longitude)
            })
            .Where(s => s.Distance < searchRadiusKm)
            .OrderBy(s => s.Distance)
            .Select(s => new LocationSearchResultDto {
                Id = s.Location.Id,
                Name = s.Location.Name,
                Type = LocationType.Shopkeeper,
                Address = s.Location.Address,
                Latitude = s.Location.Latitude,
                Longitude = s.Location.Longitude
            });
        results.AddRange(nearbyShops);
        // Return the top 5 closest results, ordered by distance
        var finalResults = results.OrderBy(r => GeoHelper.GetDistance(lat, lng, r.Latitude, r.Longitude)).Take(5).ToList();
        return Ok(finalResults);
    }


}