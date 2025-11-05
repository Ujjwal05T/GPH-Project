// GPH/Models/PlanStatus.cs
namespace GPH.Models;

public enum PlanStatus
{
    PendingApproval, // 0: Newly created by executive for a future date
    Approved,        // 1: Approved by manager (or auto-approved)
    InProgress,      // 2: At least one visit from this plan has been started
    Completed,       // 3: All visits in this plan are complete
    Cancelled        // 4: The plan was cancelled
}