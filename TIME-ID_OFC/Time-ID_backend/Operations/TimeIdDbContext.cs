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
    public DbSet<GeolocationSettingsRow> GeolocationSettings => Set<GeolocationSettingsRow>();
    public DbSet<WorkLocation> WorkLocations => Set<WorkLocation>();
    public DbSet<EmployeeLiveLocation> LiveLocations => Set<EmployeeLiveLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var stringListConverter = new ValueConverter<List<string>, string>(
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>());
        var stringListComparer = new ValueComparer<List<string>>(
            (left, right) => left != null && right != null ? left.SequenceEqual(right) : left == right,
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            value => value.ToList());
        var guidListConverter = new ValueConverter<List<Guid>, string>(
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<List<Guid>>(value, (JsonSerializerOptions?)null) ?? new List<Guid>());
        var guidListComparer = new ValueComparer<List<Guid>>(
            (left, right) => left != null && right != null ? left.SequenceEqual(right) : left == right,
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            value => value.ToList());
        var punchGeolocationConverter = new ValueConverter<PunchGeolocation?, string?>(
            value => value == null ? null : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => value == null ? null : JsonSerializer.Deserialize<PunchGeolocation>(value, (JsonSerializerOptions?)null));

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.RecoveryHashes).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            entity.HasIndex(account => account.Email);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(employee => employee.Id);
            entity.Property(employee => employee.WorkDays).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
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
            entity.Property(punch => punch.Geolocation).HasConversion(punchGeolocationConverter);
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

        modelBuilder.Entity<GeolocationSettingsRow>(entity =>
        {
            entity.ToTable("GeolocationSettings");
            entity.HasKey(settings => settings.Id);
        });

        modelBuilder.Entity<WorkLocation>(entity =>
        {
            entity.ToTable("WorkLocations");
            entity.HasKey(location => location.Id);
            entity.Property(location => location.DepartmentIds).HasConversion(guidListConverter).Metadata.SetValueComparer(guidListComparer);
            entity.Property(location => location.EmployeeIds).HasConversion(guidListConverter).Metadata.SetValueComparer(guidListComparer);
        });

        modelBuilder.Entity<EmployeeLiveLocation>(entity =>
        {
            entity.ToTable("EmployeeLiveLocations");
            entity.HasKey(location => location.EmployeeId);
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
        Id = 1, Name = company.Name, Cnpj = company.Cnpj, Email = company.Email, Phone = company.Phone, Address = company.Address
    };

    public Company ToCompany() => new() { Name = Name, Cnpj = Cnpj, Email = Email, Phone = Phone, Address = Address };
}

public sealed class GeolocationSettingsRow
{
    public int Id { get; set; } = 1;
    public int MaxCaptureAgeSeconds { get; set; } = 300;
    public int FutureToleranceSeconds { get; set; } = 30;
    public bool RequireJustification { get; set; } = true;
    public bool ManagersCanManageLocations { get; set; }
    public bool ManagersCanReview { get; set; }
    public int? RetentionDays { get; set; }
    public bool ReverseGeocodingEnabled { get; set; }

    public static GeolocationSettingsRow FromSettings(GeolocationSettings settings) => new()
    {
        Id = 1,
        MaxCaptureAgeSeconds = settings.MaxCaptureAgeSeconds,
        FutureToleranceSeconds = settings.FutureToleranceSeconds,
        RequireJustification = settings.RequireJustification,
        ManagersCanManageLocations = settings.ManagersCanManageLocations,
        ManagersCanReview = settings.ManagersCanReview,
        RetentionDays = settings.RetentionDays,
        ReverseGeocodingEnabled = settings.ReverseGeocodingEnabled
    };

    public GeolocationSettings ToSettings() => new()
    {
        MaxCaptureAgeSeconds = MaxCaptureAgeSeconds,
        FutureToleranceSeconds = FutureToleranceSeconds,
        RequireJustification = RequireJustification,
        ManagersCanManageLocations = ManagersCanManageLocations,
        ManagersCanReview = ManagersCanReview,
        RetentionDays = RetentionDays,
        ReverseGeocodingEnabled = ReverseGeocodingEnabled
    };
}
