// GPH/Controllers/ExecutivesController.cs

using GPH.Data;
using GPH.DTOs;
using GPH.Helpers;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization; // Add this if it's not already there

namespace GPH.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ExecutivesController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public ExecutivesController(ApplicationDbContext context)
    {
        _context = context;
    }
    /*
        [HttpPost]
        [Authorize(Roles = "Admin,ASM")]
        public async Task<IActionResult> CreateExecutive([FromBody] CreateSalesExecutiveDto executiveDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingUser = await _context.SalesExecutives
                    .AnyAsync(e => e.MobileNumber == executiveDto.MobileNumber || e.Username == executiveDto.Username);

            if (existingUser)
            {
                return BadRequest(new { message = "A user with this mobile number or username already exists." });
            }
            UserStatus initialStatus;
            int roleId;
            int? managerId = null;

            if (CurrentUserRole == "Admin")
            {
                roleId = executiveDto.RoleId;
                initialStatus = UserStatus.Active;
            }
            else // CurrentUserRole is "ASM"
            {
                if (executiveDto.RoleId != 3) // Assuming RoleId 1 is "Executive"
                {
                    return BadRequest(new { message = "ASMs can only create users with the Executive role." });
                }
                roleId = 3;
                initialStatus = UserStatus.PendingApproval;
                managerId = CurrentUserId;
            }

            var newExecutive = new SalesExecutive
            {
                Name = executiveDto.Name,
                MobileNumber = executiveDto.MobileNumber,
                Username = executiveDto.Username,
                Address = executiveDto.Address,
                AssignedArea = executiveDto.AssignedArea,
                Password = executiveDto.Password,
                RoleId = roleId,
                Status = initialStatus,
                ManagerId = managerId,
                        TaRatePerKm = executiveDto.TaRatePerKm // <-- ADD THIS LINE


            };

            _context.SalesExecutives.Add(newExecutive);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log the exception (ex) as needed
                return Conflict(new { message = "A user with this mobile number or username already exists.", details = ex.Message });
            }

            var createdExecutive = await _context.SalesExecutives
                .Include(e => e.Role)
                .FirstAsync(e => e.Id == newExecutive.Id);

            var resultDto = new SalesExecutiveDto
            {
                Id = createdExecutive.Id,
                Name = createdExecutive.Name,
                Username = createdExecutive.Username,
                MobileNumber = createdExecutive.MobileNumber,
                RoleName = createdExecutive.Role.Name,
                Status = createdExecutive.Status,
                AssignedArea = createdExecutive.AssignedArea,
                        TaRatePerKm = createdExecutive.TaRatePerKm // <-- ADD THIS LINE

            };

            return CreatedAtAction(nameof(GetExecutiveById), new { id = resultDto.Id }, resultDto);
        }
    */
    // In GPH/Controllers/ExecutivesController.cs

[HttpPost]
[Authorize(Roles = "Admin,ASM")]
public async Task<IActionResult> CreateExecutive([FromBody] CreateSalesExecutiveDto executiveDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    var existingUser = await _context.SalesExecutives
            .AnyAsync(e => e.MobileNumber == executiveDto.MobileNumber || e.Username == executiveDto.Username);

    if (existingUser)
    {
        return BadRequest(new { message = "A user with this mobile number or username already exists." });
    }

    UserStatus initialStatus;
    int roleId;
    int? managerId = null;
    
    // --- THIS IS THE FIX ---
    // Define the isAdmin variable and set the managerId based on the role
    bool isAdmin = CurrentUserRole == "Admin";

    if (isAdmin)
    {
        roleId = executiveDto.RoleId;
        initialStatus = UserStatus.Active;
        managerId = executiveDto.ManagerId; // Admin can set the manager from the DTO
    }
    else // CurrentUserRole is "ASM"
    {
        if (executiveDto.RoleId != 3) // Assuming RoleId 3 is "Executive"
        {
            return BadRequest(new { message = "ASMs can only create users with the Executive role." });
        }
        roleId = 3;
        initialStatus = UserStatus.PendingApproval;
        managerId = CurrentUserId; // ASM is automatically the manager
    }
    // --- END FIX ---

    var newExecutive = new SalesExecutive
    {
        Name = executiveDto.Name,
        MobileNumber = executiveDto.MobileNumber,
        Username = executiveDto.Username,
        Address = executiveDto.Address,
        AssignedArea = executiveDto.AssignedArea,
        Password = executiveDto.Password,
        RoleId = roleId,
        Status = initialStatus,
        ManagerId = managerId, // Use the correctly determined managerId
        TaRatePerKm = executiveDto.TaRatePerKm,
        DaAmount = executiveDto.DaAmount, // Assuming DaAmount is also in the DTO
        DateOfBirth = executiveDto.DateOfBirth,
        AlternatePhone = executiveDto.AlternatePhone,
        AadharNumber = executiveDto.AadharNumber,
        PanNumber = executiveDto.PanNumber,
        AccountHolderName = executiveDto.AccountHolderName,
        BankAccountNumber = executiveDto.BankAccountNumber,
        BankName = executiveDto.BankName,
        BankBranch = executiveDto.BankBranch,
        IfscCode = executiveDto.IfscCode


    };

    _context.SalesExecutives.Add(newExecutive);
    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        return Conflict(new { message = "A user with this mobile number or username already exists.", details = ex.Message });
    }

    var createdExecutive = await _context.SalesExecutives
        .Include(e => e.Role)
        .FirstAsync(e => e.Id == newExecutive.Id);

    var resultDto = new SalesExecutiveDto
    {
        Id = createdExecutive.Id,
        Name = createdExecutive.Name,
        Username = createdExecutive.Username,
        MobileNumber = createdExecutive.MobileNumber,
        RoleName = createdExecutive.Role.Name,
        Status = createdExecutive.Status,
        AssignedArea = createdExecutive.AssignedArea,
        TaRatePerKm = createdExecutive.TaRatePerKm,
        DaAmount = createdExecutive.DaAmount // Assuming DaAmount is also in the DTO
    };

    return CreatedAtAction(nameof(GetExecutiveById), new { id = resultDto.Id }, resultDto);
}

    [HttpGet]
    public async Task<IActionResult> GetAllExecutives()
    {
        var query = _context.SalesExecutives.AsQueryable();

        if (CurrentUserRole == "ASM")
        {
            query = query.Where(e => e.ManagerId == CurrentUserId);
        }

        var executives = await query
            .Include(e => e.Role)
            .Select(e => new SalesExecutiveDto
            {
                Id = e.Id,
                Name = e.Name,
                Username = e.Username,
                RoleId = e.RoleId,
                RoleName = e.Role.Name,
                Address = e.Address,



                MobileNumber = e.MobileNumber,
                Status = e.Status,
                AssignedArea = e.AssignedArea,
                TaRatePerKm = e.TaRatePerKm ,// <-- ADD THIS LINE
                        DaAmount = e.DaAmount,
                        ManagerId = e.ManagerId,
   ManagerName = e.Manager != null ? e.Manager.Name : null,
            Password = e.Password,
                DateOfBirth = e.DateOfBirth,
                AlternatePhone = e.AlternatePhone,
                AadharNumber = e.AadharNumber,
                PanNumber = e.PanNumber,
                AccountHolderName = e.AccountHolderName,
                BankAccountNumber = e.BankAccountNumber,
                BankName = e.BankName,
                BankBranch = e.BankBranch,
                IfscCode = e.IfscCode



            })
            .ToListAsync();

        return Ok(executives);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetExecutiveById(int id)
    {
        var executive = await _context.SalesExecutives
            .Include(e => e.Role)
            .Include(e => e.Manager)


            .Select(e => new SalesExecutiveDto
            {
                Id = e.Id,
                Name = e.Name,
                Username = e.Username,
                RoleId = e.RoleId,
                RoleName = e.Role.Name,
                Address = e.Address,
                AssignedArea = e.AssignedArea,




                MobileNumber = e.MobileNumber,
                Status = e.Status,
                TaRatePerKm = e.TaRatePerKm,// <-- ADD THIS LINE
                        DaAmount = e.DaAmount


                
            })
            .FirstOrDefaultAsync(e => e.Id == id);

        if (executive == null)
        {
            return NotFound();
        }

        return Ok(executive);
    }

    [HttpPost("{id}/update")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> UpdateExecutive(int id, [FromBody] UpdateSalesExecutiveDto executiveDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var executiveToUpdate = await _context.SalesExecutives.FindAsync(id);

        if (executiveToUpdate == null)
        {
            return NotFound(new { message = $"Executive with ID {id} not found." });
        }

        if (CurrentUserRole == "ASM" && executiveToUpdate.ManagerId != CurrentUserId)
        {
            return Forbid();
        }

        executiveToUpdate.Name = executiveDto.Name;
        executiveToUpdate.MobileNumber = executiveDto.MobileNumber;
        executiveToUpdate.Username = executiveDto.Username;
        executiveToUpdate.Address = executiveDto.Address;
        executiveToUpdate.AssignedArea = executiveDto.AssignedArea;
        executiveToUpdate.ManagerId = executiveDto.ManagerId; // Add this line
        executiveToUpdate.DateOfBirth = executiveDto.DateOfBirth;
    executiveToUpdate.AlternatePhone = executiveDto.AlternatePhone;
    executiveToUpdate.AadharNumber = executiveDto.AadharNumber;
    executiveToUpdate.PanNumber = executiveDto.PanNumber;
    executiveToUpdate.AccountHolderName = executiveDto.AccountHolderName;
    executiveToUpdate.BankAccountNumber = executiveDto.BankAccountNumber;
    executiveToUpdate.BankName = executiveDto.BankName;
    executiveToUpdate.BankBranch = executiveDto.BankBranch;
    executiveToUpdate.IfscCode = executiveDto.IfscCode;

        if (CurrentUserRole == "Admin")
        {
            executiveToUpdate.RoleId = executiveDto.RoleId;
            executiveToUpdate.TaRatePerKm = executiveDto.TaRatePerKm; // <-- ADD THIS LINE
            executiveToUpdate.DaAmount = executiveDto.DaAmount; // <-- ADD THIS

        }

        if (!string.IsNullOrWhiteSpace(executiveDto.Password))
        {
            executiveToUpdate.Password = executiveDto.Password;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/status")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> UpdateExecutiveStatus(int id, [FromBody] UserStatus status)
    {
        if (status == UserStatus.PendingApproval)
        {
            return BadRequest(new { message = "Cannot set status to PendingApproval via this endpoint." });
        }

        var executiveToUpdate = await _context.SalesExecutives.FindAsync(id);

        if (executiveToUpdate == null)
        {
            return NotFound(new { message = $"Executive with ID {id} not found." });
        }

        if (CurrentUserRole == "ASM" && executiveToUpdate.ManagerId != CurrentUserId)
        {
            return Forbid();
        }

        executiveToUpdate.Status = status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Executive status updated successfully.", newStatus = status.ToString() });
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveExecutive(int id)
    {
        var executiveToApprove = await _context.SalesExecutives.FindAsync(id);

        if (executiveToApprove == null)
        {
            return NotFound();
        }

        if (executiveToApprove.Status != UserStatus.PendingApproval)
        {
            return BadRequest(new { message = "User is not pending approval." });
        }

        executiveToApprove.Status = UserStatus.Active;
        await _context.SaveChangesAsync();

        return Ok(new { message = "User approved successfully." });
    }
    [HttpGet("{executiveId}/beatplan")]
public async Task<IActionResult> GetBeatPlanForExecutive(int executiveId, [FromQuery] string planDate)
{
    if (CurrentUserRole == "ASM")
    {
        var isMyExecutive = await _context.SalesExecutives.AnyAsync(e => e.Id == executiveId && e.ManagerId == CurrentUserId);
        if (!isMyExecutive) return Forbid();
    }

    if (!DateTime.TryParseExact(planDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedUtcDate))
    {
        return BadRequest(new { message = "Invalid date format. Please use YYYY-MM-DD." });
    }

    var plans = await _context.BeatPlans
        .Where(p => p.SalesExecutiveId == executiveId && p.PlanDate.Date == parsedUtcDate.Date)
        .ToListAsync();
          bool needsSave = false;
    foreach (var plan in plans)
    {
        if (plan.Status == PlanStatus.PendingApproval && plan.PlanDate == TimeZoneHelper.GetCurrentIstTime().Date)
        {
            plan.Status = PlanStatus.Approved;
            needsSave = true;
        }
    }

    if (needsSave)
    {
        await _context.SaveChangesAsync();
    }

    // --- THIS IS THE CORRECTED LOGIC ---
        // Fetch the full location objects to get all their details (name, lat, lon)
        var schoolIds = plans.Where(p => p.LocationType == LocationType.School).Select(p => p.LocationId).ToList();
    var coachingIds = plans.Where(p => p.LocationType == LocationType.CoachingCenter).Select(p => p.LocationId).ToList();
    var shopkeeperIds = plans.Where(p => p.LocationType == LocationType.Shopkeeper).Select(p => p.LocationId).ToList();

    var schools = await _context.Schools.Where(s => schoolIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);
    var coachings = await _context.CoachingCenters.Where(c => coachingIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
    var shopkeepers = await _context.Shopkeepers.Where(s => shopkeeperIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

    var resultDtos = plans.Select(p => {
        double? lat = null;
        double? lon = null;
        string name = "Unknown";

        switch (p.LocationType)
        {
            case LocationType.School:
                if (schools.TryGetValue(p.LocationId, out var school))
                {
                    name = school.Name;
                    lat = school.OfficialLatitude;
                    lon = school.OfficialLongitude;
                }
                break;
            case LocationType.CoachingCenter:
                if (coachings.TryGetValue(p.LocationId, out var coaching))
                {
                    name = coaching.Name;
                    lat = coaching.Latitude;
                    lon = coaching.Longitude;
                }
                break;
            case LocationType.Shopkeeper:
                if (shopkeepers.TryGetValue(p.LocationId, out var shopkeeper))
                {
                    name = shopkeeper.Name;
                    lat = shopkeeper.Latitude;
                    lon = shopkeeper.Longitude;
                }
                break;
        }

        return new BeatPlanDto
        {
            Id = p.Id,
            SalesExecutiveId = p.SalesExecutiveId,
            LocationId = p.LocationId,
            LocationType = p.LocationType,
            LocationName = name,
            Latitude = lat,
            Longitude = lon,
            PlanDate = p.PlanDate,
            Status = p.Status
        };
    }).ToList();
    // --- END CORRECTED LOGIC ---

    return Ok(resultDtos);
}
}