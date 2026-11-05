using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TimeId.Operations;

// Mantém o estado de trabalho em memória e persiste cada mutação em uma transação SQLite.
public class DataStore
{
    private readonly object gate = new();
    private readonly IDbContextFactory<TimeIdDbContext> contextFactory;
    private readonly string legacyPath;
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
        bool importLegacy = configuration["TIMEID_LEGACY_DATA_PATH"] is not null
            || configuration["TIMEID_DATA_PATH"] is null;

        using var context = contextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        bool databaseEmpty = !context.Accounts.Any()
            && !context.Employees.Any()
            && !context.Departments.Any()
            && !context.Punches.Any()
            && !context.FaceProfiles.Any()
            && !context.CameraSources.Any()
            && !context.CameraAlerts.Any()
            && !context.Vacations.Any()
            && !context.Holidays.Any()
            && !context.Audit.Any()
            && !context.CompanySettings.Any();

        if (databaseEmpty && importLegacy && File.Exists(legacyPath))
        {
            data = JsonSerializer.Deserialize<Database>(File.ReadAllText(legacyPath), json)
                ?? throw new InvalidDataException("Dados locais JSON inválidos.");
            if (data.Version != 1) throw new InvalidDataException("Versão dos dados locais não suportada.");
            Persist(data);
        }
        else
        {
            data = Load(context);
            if (data.Accounts.Count == 0)
            {
                SeedDemoAccounts(data);
                Persist(data);
            }
        }
    }

    private Database Copy() => JsonSerializer.Deserialize<Database>(JsonSerializer.Serialize(data, json), json)!;
    public T Read<T>(Func<Database, T> action) { lock (gate) return action(Copy()); }

    public T Write<T>(Func<Database, T> action)
    {
        lock (gate)
        {
            var next = Copy();
            var result = action(next);
            Persist(next);
            data = next;
            return result;
        }
    }

    private static Database Load(TimeIdDbContext context) => new()
    {
        Accounts = context.Accounts.AsNoTracking().ToList(),
        Employees = context.Employees.AsNoTracking().ToList(),
        Departments = context.Departments.AsNoTracking().ToList(),
        Punches = context.Punches.AsNoTracking().ToList(),
        FaceProfiles = context.FaceProfiles.AsNoTracking().ToList(),
        CameraSources = context.CameraSources.AsNoTracking().ToList(),
        CameraAlerts = context.CameraAlerts.AsNoTracking().ToList(),
        Vacations = context.Vacations.AsNoTracking().ToList(),
        Holidays = context.Holidays.AsNoTracking().ToList(),
        Audit = context.Audit.AsNoTracking().ToList(),
        Settings = context.CompanySettings.AsNoTracking().SingleOrDefault()?.ToCompany() ?? new Company()
    };

    private void Persist(Database next)
    {
        using var context = contextFactory.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        Synchronize(context, context.Accounts, next.Accounts, account => account.Id);
        Synchronize(context, context.Employees, next.Employees, employee => employee.Id);
        Synchronize(context, context.Departments, next.Departments, department => department.Id);
        Synchronize(context, context.Punches, next.Punches, punch => punch.Id);
        Synchronize(context, context.FaceProfiles, next.FaceProfiles, profile => profile.EmployeeId);
        Synchronize(context, context.CameraSources, next.CameraSources, source => source.Id);
        Synchronize(context, context.CameraAlerts, next.CameraAlerts, alert => alert.Id);
        Synchronize(context, context.Vacations, next.Vacations, vacation => vacation.Id);
        Synchronize(context, context.Holidays, next.Holidays, holiday => holiday.Id);
        Synchronize(context, context.Audit, next.Audit, change => change.Id);
        var settings = context.CompanySettings.SingleOrDefault();
        if (settings is null)
            context.CompanySettings.Add(CompanySettingsRow.FromCompany(next.Settings));
        else
            context.Entry(settings).CurrentValues.SetValues(CompanySettingsRow.FromCompany(next.Settings));

        context.SaveChanges();
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

    private static void SeedDemoAccounts(Database database)
    {
        var hasher = new PasswordHasher<Account>();
        foreach (var (name, email, role) in new[] { ("Usuario de teste", "teste@timeid.local", "Administrador"), ("Usuario comum", "usuario@timeid.local", "Usuário") })
        {
            var account = new Account { Username = name, Email = email, Role = role };
            account.PasswordHash = hasher.HashPassword(account, "TimeId@123");
            database.Accounts.Add(account);
        }
    }

    public static Account Actor(Database db, Guid id) => db.Accounts.FirstOrDefault(a => a.Id == id && a.Active) ?? throw new DomainError(401, "Sessão inválida.");
    public static void Admin(Account actor) { if (actor.Role != "Administrador") throw new DomainError(403, "Somente administradores podem executar esta ação."); }
    public static void Log(Database db, Account actor, string action, string entity, string detail) =>
        db.Audit.Add(new(Guid.NewGuid(), DateTimeOffset.UtcNow, actor.Id, actor.Username, action, entity, detail));
    public static DateOnly Today => Date(DateTimeOffset.UtcNow);
    public static DateOnly Date(DateTimeOffset at) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(at, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")).DateTime);
}
