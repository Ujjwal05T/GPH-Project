// GPH/Controllers/AuthController.cs

using System.Security.Claims;
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GPH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var executive = await _context.SalesExecutives
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.MobileNumber == loginDto.MobileNumber);

        if (executive == null || executive.Password != loginDto.Password)
        {
            return Unauthorized(new { message = "Invalid mobile number or password." });
        }

        // Use the new Status property for the check
        if (executive.Status != UserStatus.Active)
        {
            return Unauthorized(new { message = "Your account is not active or is pending approval." });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, executive.Id.ToString()),
            new Claim(ClaimTypes.Name, executive.Name),
            new Claim(ClaimTypes.Role, executive.Role.Name)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        var userDto = new SalesExecutiveDto
        {
            Id = executive.Id,
            Name = executive.Name,
            MobileNumber = executive.MobileNumber,
            RoleName = executive.Role.Name,
            Status = executive.Status // Use the new Status property
        };

        return Ok(userDto);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logout successful." });
    }

    // This endpoint can be used by the frontend to check if a user is still logged in
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var executive = await _context.SalesExecutives
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Id == userId);

        // Check the user's status in the database
        if (executive == null || executive.Status != UserStatus.Active)
        {
            // If the user has been deactivated since they logged in, sign them out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized();
        }

        var userDto = new SalesExecutiveDto
        {
            Id = executive.Id,
            Name = executive.Name,
            MobileNumber = executive.MobileNumber,
            RoleName = executive.Role.Name,
            Status = executive.Status // Use the new Status property
        };

        return Ok(userDto);
    }
}