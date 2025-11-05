// GPH/Models/UserStatus.cs
namespace GPH.Models;

public enum UserStatus
{
    PendingApproval, // Newly created by an ASM
    Active,          // Approved and working
    Deactivated      // Deactivated by a manager
}