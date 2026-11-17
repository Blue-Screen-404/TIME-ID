using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TimeId.Operations;

public sealed class TimeIdDbContext(DbContextOptions<TimeIdDbContext> options) : DbContext(options)
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
    public DbSet<CompanySettingsRow> CompanySettings => Set<CompanySettingsRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var listConverter = new ValueConverter<List<string>, string>(
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>());
        var listComparer = new ValueComparer<List<string>>(
            (left, right) => left != null && right != null ? left.SequenceEqual(right) : left == right,
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            value => value.ToList());

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.RecoveryHashes).HasConversion(listConverter).Metadata.SetValueComparer(listComparer);
            entity.HasIndex(account => account.Email);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(employee => employee.Id);
            entity.Property(employee => employee.WorkDays).HasConversion(listConverter).Metadata.SetValueComparer(listComparer);
            entity.HasIndex(employee => employee.DepartmentId);
            entity.HasIndex(employee => employee.LeaderId);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(department => department.Id);
        });

        modelBuilder.Entity<Punch>(entity =>
        {
            entity.ToTable("Punches");
            entity.HasKey(punch => punch.Id);
            entity.HasIndex(punch => new { punch.EmployeeId, punch.At });
        });

        modelBuilder.Entity<FaceProfile>(entity =>
        {
            entity.ToTable("FaceProfiles");
            entity.HasKey(profile => profile.EmployeeId);
        });

        modelBuilder.Entity<CameraSource>(entity =>
        {
            entity.ToTable("CameraSources");
            entity.HasKey(source => source.Id);
        });

        modelBuilder.Entity<CameraAlert>(entity =>
        {
            entity.ToTable("CameraAlerts");
            entity.HasKey(alert => alert.Id);
            entity.HasIndex(alert => alert.At);
        });

        modelBuilder.Entity<Leave>(entity =>
        {
            entity.ToTable("Vacations");
            entity.HasKey(vacation => vacation.Id);
            entity.HasIndex(vacation => vacation.EmployeeId);
        });

        modelBuilder.Entity<DayOff>(entity =>
        {
            entity.ToTable("Holidays");
            entity.HasKey(holiday => holiday.Id);
        });

        modelBuilder.Entity<Change>(entity =>
        {
            entity.ToTable("Audit");
            entity.HasKey(change => change.Id);
            entity.HasIndex(change => change.At);
        });

        modelBuilder.Entity<CompanySettingsRow>(entity =>
        {
            entity.ToTable("CompanySettings");
            entity.HasKey(settings => settings.Id);
        });
    }
}

public sealed class CompanySettingsRow
{
    public int Id { get; set; } = 1;
    public string Name { get; set; } = "TIMEID";
    public string Cnpj { get; set; } = "";
    public string Email { get; set; } = "contato@timeid.local";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";

    public static CompanySettingsRow FromCompany(Company company) => new()
    {
        Id = 1,
        Name = company.Name,
        Cnpj = company.Cnpj,
        Email = company.Email,
        Phone = company.Phone,
        Address = company.Address
    };

    public Company ToCompany() => new()
    {
        Name = Name,
        Cnpj = Cnpj,
        Email = Email,
        Phone = Phone,
        Address = Address
    };
}
