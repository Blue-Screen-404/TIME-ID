
using Microsoft.EntityFrameworkCore;

namespace TimeId.Operations;

public sealed class TimeIdDbContext(
    DbContextOptions<TimeIdDbContext> options
) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Staff> Employees => Set<Staff>();
    public DbSet<Team> Departments => Set<Team>();
    public DbSet<Punch> Punches => Set<Punch>();
    public DbSet<FaceProfile> FaceProfiles => Set<FaceProfile>();
    public DbSet<CameraSource> CameraSources => Set<CameraSource>();
    public DbSet<CameraAlert> CameraAlerts => Set<CameraAlert>();
    public DbSet<Leave> Vacations => Set<Leave>();
    public DbSet<DayOff> Holidays => Set<DayOff>();
    public DbSet<Change> Audit => Set<Change>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FaceProfile>()
            .HasKey(f => f.EmployeeId);

        modelBuilder.Entity<Change>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<CameraAlert>()
            .HasKey(c => c.Id);
    }
}
