// GPH/Data/ApplicationDbContext.cs

using GPH.Models;
using Microsoft.EntityFrameworkCore;

namespace GPH.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<SalesExecutive> SalesExecutives { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Visit> Visits { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookDistribution> BookDistributions { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Order> Orders { get; set; }
        public DbSet<VisitDetail> VisitDetails { get; set; }

    public DbSet<InventoryAssignment> InventoryAssignments { get; set; }
public DbSet<BeatPlan> BeatPlans { get; set; }
public DbSet<DailyTracking> DailyTrackings { get; set; }
public DbSet<LocationPoint> LocationPoints { get; set; }

public DbSet<Role> Roles { get; set; }

public DbSet<Consignment> Consignments { get; set; }
public DbSet<ConsignmentItem> ConsignmentItems { get; set; }
public DbSet<CoachingCenter> CoachingCenters { get; set; }
public DbSet<Shopkeeper> Shopkeepers { get; set; }
public DbSet<BeatAssignment> BeatAssignments { get; set; }
public DbSet<MonthlyTask> MonthlyTasks { get; set; } // <-- ADD THIS LINE
public DbSet<TrackingSession> TrackingSessions { get; set; }



    // This method is essential for fine-tuning the database model.



    // In Data/ApplicationDbContext.cs

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Book>(entity =>
   {
       entity.HasIndex(e => e.Title)
             .IsUnique();
   });
    modelBuilder.Entity<LocationPoint>()
                .ToTable(tb => tb.HasTrigger("trg_DeleteZeroLatitude"));

        // --- FIX FOR MULTIPLE CASCADE PATHS ---
        // (This part is from our previous fix and is still needed)
        modelBuilder.Entity<Teacher>()
            .HasOne(t => t.School)
            .WithMany(s => s.Teachers)
            .HasForeignKey(t => t.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<BookDistribution>()
            .HasOne(bd => bd.Visit)
            .WithMany()
            .HasForeignKey(bd => bd.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookDistribution>()
            .HasOne(bd => bd.Teacher)
            .WithMany()
            .HasForeignKey(bd => bd.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- NEW EXPLICIT CONFIGURATION FOR ORDER ---
        // This will resolve the shadow property warning.
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Book) // An Order has one Book
            .WithMany()          // A Book can be in many Orders
            .HasForeignKey(o => o.BookId) // Use the 'BookId' property as the foreign key
            .IsRequired();       // Make the relationship mandatory

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Visit) // An Order has one Visit
            .WithMany()           // A Visit can have many Orders
            .HasForeignKey(o => o.VisitId) // Use the 'VisitId' property as the foreign key
            .IsRequired();
        // modelBuilder.Entity<SalesExecutive>()
        // .HasIndex(e => e.Username)
        // .IsUnique();
         modelBuilder.Entity<SalesExecutive>()
        .HasOne(e => e.Role)
        .WithMany()
        .HasForeignKey(e => e.RoleId)
        .OnDelete(DeleteBehavior.Restrict); // Use Restrict for safety

       modelBuilder.Entity<SalesExecutive>()
        .HasIndex(e => e.MobileNumber)
        .IsUnique();
 // This breaks the cascade cycle
        // --- ADD THIS SEEDING LOGIC ---

        // In Data/ApplicationDbContext.cs -> OnModelCreating method

        // --- UPDATE THIS SEEDING LOGIC ---
        modelBuilder.Entity<Role>().HasData(
    new Role { Id = 1, Name = "Admin" },
    new Role { Id = 2, Name = "ASM" },
    new Role { Id = 3, Name = "Executive" }
);
}
}