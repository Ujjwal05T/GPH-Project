// GPH/Services/DailyAllowanceService.cs

using GPH.Data;
using GPH.Models;
using Microsoft.EntityFrameworkCore;

namespace GPH.Services;

public class DailyAllowanceService : IDailyAllowanceService
{
    private readonly ApplicationDbContext _context;
    
    // Define the DA amount as a constant
    private const decimal DailyAllowanceAmount = 300.0m;

    public DailyAllowanceService(ApplicationDbContext context)
    {
        _context = context;
    }

  public async Task<bool> CheckAndAwardExecutiveDA(int executiveId, DateTime date)
{
    var today = date.Date;
    var daExists = await _context.Expenses.AnyAsync(e => e.SalesExecutiveId == executiveId && e.Type == ExpenseType.DailyAllowance && e.ExpenseDate.Date == today);
    if (daExists) return false; // Already awarded, do nothing

    var visits = await _context.Visits
        .Where(v => v.SalesExecutiveId == executiveId && v.CheckInTimestamp.Date == today)
        .Select(v => v.LocationType).ToListAsync();
    
    if (visits.Count(vt => vt == LocationType.School) >= 4 &&
        visits.Count(vt => vt == LocationType.CoachingCenter) >= 1 &&
        visits.Count(vt => vt == LocationType.Shopkeeper) >= 1)
    {
        // --- THIS IS THE FIX ---
        // 1. Fetch the executive from the database to get their specific DA rate.
        var executive = await _context.SalesExecutives.FindAsync(executiveId);
        if (executive == null)
        {
            // This should not happen if the executiveId is valid, but it's a good safety check.
            return false; 
        }

        // 2. Create the expense using the dynamic amount from the executive's profile.
        var daExpense = new Expense 
        {
            SalesExecutiveId = executiveId,
            Type = ExpenseType.DailyAllowance,
            Amount = executive.DaAmount, // Use the dynamic amount
            ExpenseDate = DateTime.UtcNow,
            Description = "Automated DA for target completion.",
            Status = ApprovalStatus.Pending
        };
        // --- END FIX ---

        _context.Expenses.Add(daExpense);
        await _context.SaveChangesAsync();
        return true; // DA was just awarded
    }
    return false;
}

    public async Task CheckAndAwardAsmDA(int executiveId, DateTime date)
    {
        var today = date.Date;
        var executive = await _context.SalesExecutives.FindAsync(executiveId);
        if (executive?.ManagerId == null) return; // No manager, nothing to do

        var managerId = executive.ManagerId.Value;

        // Check if the ASM's DA has already been awarded for today
        var asmDaExists = await _context.Expenses.AnyAsync(e => e.SalesExecutiveId == managerId && e.Type == ExpenseType.DailyAllowance && e.ExpenseDate.Date == today);
        if (asmDaExists) return;

        // Get all active executives managed by this ASM
        var teamMemberIds = await _context.SalesExecutives
            .Where(e => e.ManagerId == managerId && e.Status == UserStatus.Active)
            .Select(e => e.Id)
            .ToListAsync();

        if (!teamMemberIds.Any()) return; // No active team members

        // Check if EVERY team member has completed their DA target for today
        foreach (var memberId in teamMemberIds)
        {
            var memberVisits = await _context.Visits
                .Where(v => v.SalesExecutiveId == memberId && v.CheckInTimestamp.Date == today)
                .Select(v => v.LocationType).ToListAsync();
            
            bool memberCompleted = memberVisits.Count(vt => vt == LocationType.School) >= 4 &&
                                   memberVisits.Count(vt => vt == LocationType.CoachingCenter) >= 1 &&
                                   memberVisits.Count(vt => vt == LocationType.Shopkeeper) >= 1;

            if (!memberCompleted)
            {
                // If even one member has not completed their target, the ASM does not get their DA yet.
                return; 
            }
        }

        // If the loop completes, it means everyone has met their target. Award the ASM's DA.
        var asmDaExpense = new Expense
        {
            SalesExecutiveId = managerId,
            Type = ExpenseType.DailyAllowance,
            Amount = DailyAllowanceAmount, // Assuming ASM gets the same DA amount
            ExpenseDate = DateTime.UtcNow,
            Description = $"Automated DA for team completion.",
            Status = ApprovalStatus.Pending
        };
        _context.Expenses.Add(asmDaExpense);
        await _context.SaveChangesAsync();
    }
}