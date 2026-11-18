using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TimeId.Operations;

// Mantém uma cópia de trabalho em memória e persiste cada mutação em uma transação SQLite.
public class DataStore
{
    private readonly object gate = new();
    private readonly IDbContextFactory<TimeIdDbContext> contextFactory;
    private readonly string legacyPath;
    private readonly bool importLegacy;
    private readonly JsonSerializerOptions json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private Database data;

    public DataStore(
        IDbContextFactory<TimeIdDbContext> contextFactory,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        this.contextFactory = contextFactory;
        string configuredLegacyPath = configuration["TIMEID_LEGACY_DATA_PATH"]
            ?? Path.Combine(environment.ContentRootPath, "App_Data", "timeid.json");
        legacyPath = Path.GetFullPath(Path.IsPathRooted(configuredLegacyPath)
            ? configuredLegacyPath
            : Path.Combine(environment.ContentRootPath, configuredLegacyPath));
        importLegacy = configuration["TIMEID_LEGACY_DATA_PATH"] is not null
            || configuration["TIMEID_DATA_PATH"] is null;

        using var db = contextFactory.CreateDbContext();
        AdoptDatabaseCreatedWithoutMigrations(db);
        db.Database.Migrate();
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS TimeIdState (Key TEXT NOT NULL PRIMARY KEY, Value TEXT NOT NULL)");
        RestoreLegacySettings(db);

        bool databaseEmpty = !db.Accounts.Any()
            && !db.Employees.Any()
            && !db.Departments.Any()
            && !db.Punches.Any()
            && !db.FaceProfiles.Any()
            && !db.CameraSources.Any()
            && !db.CameraAlerts.Any()
            && !db.Vacations.Any()
            && !db.Holidays.Any()
            && !db.Audit.Any()
            && !db.CompanySettings.Any()
            && !db.GeolocationSettings.Any()
            && !db.WorkLocations.Any()
            && !db.LiveLocations.Any();

        if (databaseEmpty && importLegacy && File.Exists(legacyPath))
        {
            data = JsonSerializer.Deserialize<Database>(File.ReadAllText(legacyPath), json)
                ?? throw new InvalidDataException("Dados locais JSON inválidos.");
            if (data.Version != 1) throw new InvalidDataException("Versão dos dados locais não suportada.");
            Persist(data, initialize: true);
        }
        else
        {
            data = Load(db);
            if (data.Accounts.Count == 0)
            {
                SeedDemoAccounts(data);
                Persist(data, initialize: true);
            }
        }

        data = Load(db);
    }

    private static void AdoptDatabaseCreatedWithoutMigrations(TimeIdDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        bool wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen) connection.Open();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('Accounts', 'Employees', 'Departments', 'Punches', 'FaceProfiles', 'CameraSources', 'CameraAlerts', 'Vacations', 'Holidays', 'Audit')";
            long existingTables = Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
            if (existingTables != 10) return;

            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsHistory'";
            bool historyExists = Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) > 0;
            if (historyExists)
            {
                command.CommandText = "SELECT COUNT(*) FROM __EFMigrationsHistory";
                if (Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) > 0) return;
            }

            command.CommandText = "CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (MigrationId TEXT NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY, ProductVersion TEXT NOT NULL)";
            command.ExecuteNonQuery();
            command.CommandText = "INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20261107022607_InitialCreate', '8.0.32')";
            command.ExecuteNonQuery();
        }
        finally
        {
            if (!wasOpen) connection.Close();
        }
    }

    private static void RestoreLegacySettings(TimeIdDbContext db)
    {
        if (!db.CompanySettings.Any())
        {
            string? savedCompany = ReadState(db, "Company");
            if (savedCompany is not null)
            {
                Company company = JsonSerializer.Deserialize<Company>(savedCompany, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? new Company();
                db.CompanySettings.Add(CompanySettingsRow.FromCompany(company));
            }
        }

        if (!db.GeolocationSettings.Any())
        {
            string? savedGeolocation = ReadState(db, "GeolocationSettings");
            if (savedGeolocation is not null)
            {
                GeolocationSettings settings = JsonSerializer.Deserialize<GeolocationSettings>(savedGeolocation, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? new GeolocationSettings();
                db.GeolocationSettings.Add(GeolocationSettingsRow.FromSettings(settings));
            }
        }

        db.SaveChanges();
    }

    private Database Copy() => JsonSerializer.Deserialize<Database>(JsonSerializer.Serialize(data, json), json)!;

    public T Read<T>(Func<Database, T> action)
    {
        lock (gate) return action(Copy());
    }

    public T Write<T>(Func<Database, T> action)
    {
        lock (gate)
        {
            var next = Copy();
            T result = action(next);
            Persist(next, initialize: false);
            data = next;
            return result;
        }
    }

    private void Persist(Database next, bool initialize)
    {
        using var db = contextFactory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction();

        Synchronize(db, db.Accounts, next.Accounts, account => account.Id);
        Synchronize(db, db.Employees, next.Employees, employee => employee.Id);
        Synchronize(db, db.Departments, next.Departments, department => department.Id);
        Synchronize(db, db.Punches, next.Punches, punch => punch.Id);
        Synchronize(db, db.FaceProfiles, next.FaceProfiles, profile => profile.EmployeeId);
        Synchronize(db, db.CameraSources, next.CameraSources, source => source.Id);
        Synchronize(db, db.CameraAlerts, next.CameraAlerts, alert => alert.Id);
        Synchronize(db, db.Vacations, next.Vacations, vacation => vacation.Id);
        Synchronize(db, db.Holidays, next.Holidays, holiday => holiday.Id);
        Synchronize(db, db.Audit, next.Audit, change => change.Id);
        Synchronize(db, db.WorkLocations, next.WorkLocations, location => location.Id);
        Synchronize(db, db.LiveLocations, next.LiveLocations, location => location.EmployeeId);

        var companySettings = db.CompanySettings.SingleOrDefault();
        if (companySettings is null)
            db.CompanySettings.Add(CompanySettingsRow.FromCompany(next.Settings));
        else
            db.Entry(companySettings).CurrentValues.SetValues(CompanySettingsRow.FromCompany(next.Settings));

        var geolocationSettings = db.GeolocationSettings.SingleOrDefault();
        if (geolocationSettings is null)
            db.GeolocationSettings.Add(GeolocationSettingsRow.FromSettings(next.GeolocationSettings));
        else
            db.Entry(geolocationSettings).CurrentValues.SetValues(GeolocationSettingsRow.FromSettings(next.GeolocationSettings));

        db.SaveChanges();
        WriteState(db, "Version", next.Version.ToString(System.Globalization.CultureInfo.InvariantCulture));
        WriteState(db, "Company", JsonSerializer.Serialize(next.Settings, json));
        WriteState(db, "GeolocationSettings", JsonSerializer.Serialize(next.GeolocationSettings, json));
        if (initialize) WriteState(db, "Initialized", "1");
        transaction.Commit();
    }

    private static void Synchronize<TEntity, TKey>(
        TimeIdDbContext context,
        DbSet<TEntity> table,
        IEnumerable<TEntity> rows,
        Func<TEntity, TKey> keySelector)
        where TEntity : class
        where TKey : notnull
    {
        var pending = rows.ToDictionary(keySelector);
        foreach (var existing in table.ToList())
        {
            if (pending.Remove(keySelector(existing), out var updated))
                context.Entry(existing).CurrentValues.SetValues(updated);
            else
                table.Remove(existing);
        }
        table.AddRange(pending.Values);
    }

    private static Database Load(TimeIdDbContext db) => new()
    {
        Accounts = db.Accounts.AsNoTracking().ToList(),
        Employees = db.Employees.AsNoTracking().ToList(),
        Departments = db.Departments.AsNoTracking().ToList(),
        Punches = db.Punches.AsNoTracking().ToList(),
        LiveLocations = db.LiveLocations.AsNoTracking().ToList(),
        FaceProfiles = db.FaceProfiles.AsNoTracking().ToList(),
        CameraSources = db.CameraSources.AsNoTracking().ToList(),
        CameraAlerts = db.CameraAlerts.AsNoTracking().ToList(),
        Vacations = db.Vacations.AsNoTracking().ToList(),
        Holidays = db.Holidays.AsNoTracking().ToList(),
        Audit = db.Audit.AsNoTracking().ToList(),
        WorkLocations = db.WorkLocations.AsNoTracking().ToList(),
        GeolocationSettings = db.GeolocationSettings.AsNoTracking().SingleOrDefault()?.ToSettings() ?? LoadLegacyGeolocationSettings(db),
        Settings = db.CompanySettings.AsNoTracking().SingleOrDefault()?.ToCompany() ?? LoadLegacyCompany(db),
        Version = int.TryParse(ReadState(db, "Version"), out int version) ? version : 1
    };

    private static Company LoadLegacyCompany(TimeIdDbContext db)
    {
        string? value = ReadState(db, "Company");
        return value is null
            ? new Company()
            : JsonSerializer.Deserialize<Company>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new Company();
    }

    private static GeolocationSettings LoadLegacyGeolocationSettings(TimeIdDbContext db)
    {
        string? value = ReadState(db, "GeolocationSettings");
        return value is null
            ? new GeolocationSettings()
            : JsonSerializer.Deserialize<GeolocationSettings>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new GeolocationSettings();
    }

    private static string? ReadState(TimeIdDbContext db, string key)
    {
        var connection = db.Database.GetDbConnection();
        bool wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen) connection.Open();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM TimeIdState WHERE Key = @key";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@key";
            parameter.Value = key;
            command.Parameters.Add(parameter);
            return command.ExecuteScalar() as string;
        }
        finally
        {
            if (!wasOpen) connection.Close();
        }
    }

    private static void WriteState(TimeIdDbContext db, string key, string value) => db.Database.ExecuteSqlInterpolated(
        $"INSERT INTO TimeIdState (Key, Value) VALUES ({key}, {value}) ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value");

    private static void SeedDemoAccounts(Database database)
    {
        var hasher = new PasswordHasher<Account>();
        foreach (var (name, email, role) in new[] {
            ("Usuario de teste", "teste@timeid.local", "Administrador"),
            ("Usuario comum", "usuario@timeid.local", "Usuário") })
        {
            var account = new Account { Username = name, Email = email, Role = role };
            account.PasswordHash = hasher.HashPassword(account, "TimeId@123");
            database.Accounts.Add(account);
        }
    }

    public static Account Actor(Database db, Guid id) => db.Accounts.FirstOrDefault(a => a.Id == id && a.Active)
        ?? throw new DomainError(401, "Sessão inválida.");
    public static void Admin(Account actor)
    {
        if (actor.Role != "Administrador") throw new DomainError(403, "Somente administradores podem executar esta ação.");
    }
    public static void Log(Database db, Account actor, string action, string entity, string detail) =>
        db.Audit.Add(new(Guid.NewGuid(), DateTimeOffset.UtcNow, actor.Id, actor.Username, action, entity, detail));
    public static DateOnly Today => Date(DateTimeOffset.UtcNow);
    public static DateOnly Date(DateTimeOffset at) => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTime(at, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")).DateTime);
}
