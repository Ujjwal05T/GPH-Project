// GPH/Controllers/ExpensesController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GPH.Controllers;

[Authorize] // Protect the entire controller
[Route("api/[controller]")]
public class ExpensesController : BaseApiController // Inherit from BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;
    public ExpensesController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
    }
    // POST: /api/expenses (For manual "Other" expenses)
    [HttpPost]
    public async Task<IActionResult> CreateExpense([FromForm] CreateExpenseDto expenseDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        string? billUrl = null;
        // --- File Upload Logic ---
        if (expenseDto.BillFile != null && expenseDto.BillFile.Length > 0)
        {
            // We save files in the wwwroot/uploads/expenses folder
            var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "expenses");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{expenseDto.BillFile.FileName}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await expenseDto.BillFile.CopyToAsync(stream);
            }
            // Generate the public URL for the file
            billUrl = $"{Request.Scheme}://{Request.Host}/uploads/expenses/{uniqueFileName}";
        }
        var newExpense = new Expense
        {
            SalesExecutiveId = expenseDto.SalesExecutiveId,
            Type = (ExpenseType)expenseDto.Type, // Cast int to enum
            Amount = expenseDto.Amount,
            ExpenseDate = expenseDto.ExpenseDate,
            Description = expenseDto.Description,
            Status = ApprovalStatus.Pending,
            BillUrl = billUrl // Save the URL to the database
        };
        _context.Expenses.Add(newExpense);
        await _context.SaveChangesAsync();
        // It's good practice to return the created object
        var createdExpense = await _context.Expenses
            .Include(e => e.SalesExecutive)
            .FirstAsync(e => e.Id == newExpense.Id);
        var resultDto = new ExpenseDto
        {
            Id = createdExpense.Id,
            SalesExecutiveId = createdExpense.SalesExecutiveId,
            SalesExecutiveName = createdExpense.SalesExecutive.Name,
            Type = createdExpense.Type,
            Amount = createdExpense.Amount,
            ExpenseDate = createdExpense.ExpenseDate,
            Description = createdExpense.Description,
            Status = createdExpense.Status,
            BillUrl = createdExpense.BillUrl // Return the new URL
        };
        return Ok(resultDto);
    }
    // GET: /api/executives/{executiveId}/expenses
    [HttpGet("/api/executives/{executiveId}/expenses")]
    public async Task<IActionResult> GetExpensesForExecutive(int executiveId)
    {
         // --- NEW SECURITY LOGIC ---
    // An executive can only see their own expenses.
    if (CurrentUserRole == "Executive" && CurrentUserId != executiveId)
    {
        return Forbid();
    }
    // An ASM can only see their own team's expenses.
    if (CurrentUserRole == "ASM")
    {
        var isMyExecutive = await _context.SalesExecutives
            .AnyAsync(e => e.Id == executiveId && e.ManagerId == CurrentUserId);
        if (!isMyExecutive && executiveId != CurrentUserId) // ASM can also see their own
        {
            return Forbid();
        }
    }
    // Admins can see anyone's expenses.
    // --- END SECURITY LOGIC ---
        var expenses = await _context.Expenses
            .Include(e => e.SalesExecutive)
            .Where(e => e.SalesExecutiveId == executiveId)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                SalesExecutiveId = e.SalesExecutiveId,
                SalesExecutiveName = e.SalesExecutive.Name,
                Type = e.Type,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,
                Status = e.Status
            })
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
        return Ok(expenses);
    }
    // GET: /api/expenses/pending (For the new Approvals page)
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> GetPendingExpenses()
    {
        var query = _context.Expenses
            .Include(e => e.SalesExecutive)
            .Where(e => e.Status == ApprovalStatus.Pending);
        if (CurrentUserRole == "ASM")
        {
            // An ASM can only see pending expenses from their own team
            query = query.Where(e => e.SalesExecutive.ManagerId == CurrentUserId);
        }
        var pendingExpenses = await query
            .OrderBy(e => e.ExpenseDate)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                SalesExecutiveId = e.SalesExecutiveId,
                SalesExecutiveName = e.SalesExecutive.Name,
                Type = e.Type,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,
                Status = e.Status,
                BillUrl = e.BillUrl // << --- YEH LINE ADD KARNA ZAROORI HAI ---
            })
            .ToListAsync();
        return Ok(pendingExpenses);
    }
    // PUT: /api/expenses/{id}/approval (Handles both Approve and Reject)
    //[HttpPut("{id}/approval")]
    [HttpPost("{id}/approval")]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> UpdateExpenseApproval(int id, [FromBody] ApprovalStatus status)
    {
        if (status == ApprovalStatus.Pending)
        {
            return BadRequest(new { message = "Cannot set status to Pending." });
        }
        var expenseToUpdate = await _context.Expenses.FindAsync(id);
        if (expenseToUpdate == null)
        {
            return NotFound();
        }
        // Security check: An ASM can only approve expenses for their own team
        if (CurrentUserRole == "ASM")
        {
            var executive = await _context.SalesExecutives.FindAsync(expenseToUpdate.SalesExecutiveId);
            if (executive?.ManagerId != CurrentUserId)
            {
                return Forbid();
            }
        }
        expenseToUpdate.Status = status;
        expenseToUpdate.ApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Expense status updated to {status}." });
    }
    

}
