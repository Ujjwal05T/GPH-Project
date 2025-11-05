// GPH/Controllers/BaseApiController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    // Get the role as a string directly from the claim
    protected string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}