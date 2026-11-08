using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TimeId.Operations;

// Persistencia SQLite em uma unica instancia. Mantem a API Read/Write dos servicos.
// Cada escrita usa transacao: o estado em memoria so muda apos o commit.
public class DataStore
{
    private readonly object gate = new();
    private readonly IDbContextFactory<TimeIdDbContext> factory;
    private readonly JsonSerializerOptions json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private Database data;

    public DataStore(IWebHostEnvironment environment, IConfiguration configuration,
        IDbContextFactory<TimeIdDbContext> contextFactory)
    {
        factory = contextFactory;
        using var db = factory.CreateDbContext();
        // Aplicar apenas migracoes existentes. Nao usar EnsureCreated com migrations.
        db.Database.Migrate();
        db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS TimeIdState (Key TEXT NOT NULL PRIMARY KEY, Value TEXT NOT NULL)");

        data = Load(db);
        bool initialized = ReadState(db, "Initialized") is not null;
        if (!initialized)
        {
            // Nunca sobrescrever um banco que ja possua linhas.
            bool populated = db.Accounts.Any() || db.Employees.Any() ||
                db.Departments.Any() || db.Punches.Any() || db.FaceProfiles.Any() ||
                db.CameraSources.Any() || db.CameraAlerts.Any() ||
                db.Vacations.Any() || db.Holidays.Any() || db.Audit.Any();

            if (populated)
            {
                // Banco preexistente: conservar tudo e apenas registrar inicializacao.
                using var transaction = db.Database.BeginTransaction();
                WriteState(db, "Initialized", "1");
                transaction.Commit();
            }
            else
            {
                string legacyPath = Path.GetFullPath(configuration["TIMEID_DATA_PATH"] ??
                    Path.Combine(environment.ContentRootPath, "App_Data", "timeid.json"));
                if (File.Exists(legacyPath))
                {
                    // O JSON original permanece intacto para backup/rollback.
                    data = JsonSerializer.Deserialize<Database>(File.ReadAllText(legacyPath), json)
                        ?? throw new InvalidDataException("Dados JSON invalidos.");
                    if (data.Version != 1)
                        throw new InvalidDataException("Versao de dados JSON nao suportada.");
                }
                else
                {
                    data = new Database();
                    var hasher = new PasswordHasher<Account>();
                    foreach (var (name, email, role) in new[] {
                        ("Usuario de teste", "teste@timeid.local", "Administrador"),
                        ("Usuario comum", "usuario@timeid.local", "Usuário") })
                    {
                        var account = new Account { Username = name, Email = email, Role = role };
                        account.PasswordHash = hasher.HashPassword(account, "TimeId@123");
                        data.Accounts.Add(account);
                    }
                }
                Persist(data, initialize: true);
            }
        }
        data = Load(db);
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
        using var db = factory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction();
        // Compatibilidade com as operacoes atuais que alteram List<T> em memoria.
        // Substituicao integral em uma unica transacao. Adequado apenas para
        // instalacoes locais de pequeno porte, nao para grande volume/multiplos processos.
        db.Accounts.RemoveRange(db.Accounts);
        db.Employees.RemoveRange(db.Employees);
        db.Departments.RemoveRange(db.Departments);
        db.Punches.RemoveRange(db.Punches);
        db.FaceProfiles.RemoveRange(db.FaceProfiles);
        db.CameraSources.RemoveRange(db.CameraSources);
        db.CameraAlerts.RemoveRange(db.CameraAlerts);
        db.Vacations.RemoveRange(db.Vacations);
        db.Holidays.RemoveRange(db.Holidays);
        db.Audit.RemoveRange(db.Audit);
        db.SaveChanges();

        db.Accounts.AddRange(next.Accounts);
        db.Employees.AddRange(next.Employees);
        db.Departments.AddRange(next.Departments);
        db.Punches.AddRange(next.Punches);
        db.FaceProfiles.AddRange(next.FaceProfiles);
        db.CameraSources.AddRange(next.CameraSources);
        db.CameraAlerts.AddRange(next.CameraAlerts);
        db.Vacations.AddRange(next.Vacations);
        db.Holidays.AddRange(next.Holidays);
        db.Audit.AddRange(next.Audit);
        db.SaveChanges();

        WriteState(db, "Company", JsonSerializer.Serialize(next.Settings, json));
        WriteState(db, "Version", next.Version.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (initialize) WriteState(db, "Initialized", "1");
        transaction.Commit();
    }

    private static Database Load(TimeIdDbContext db)
    {
            var state = new Database
            {
                Accounts = db.Accounts.AsNoTracking().ToList(),
                Employees = db.Employees.AsNoTracking().ToList(),
                Departments = db.Departments.AsNoTracking().ToList(),
                Punches = db.Punches.AsNoTracking().ToList(),
                FaceProfiles = db.FaceProfiles.AsNoTracking().ToList(),
                CameraSources = db.CameraSources.AsNoTracking().ToList(),
                CameraAlerts = db.CameraAlerts.AsNoTracking().ToList(),
                Vacations = db.Vacations.AsNoTracking().ToList(),
                Holidays = db.Holidays.AsNoTracking().ToList(),
                Audit = db.Audit.AsNoTracking().ToList()
            };
            string? company = ReadState(db, "Company");
            if (company is not null)
                state.Settings = JsonSerializer.Deserialize<Company>(company, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? new Company();
            string? version = ReadState(db, "Version");
            if (version is not null && int.TryParse(version, out int v)) state.Version = v;
            return state;
    }

    private static string? ReadState(TimeIdDbContext db, string key)
    {
        var conn = db.Database.GetDbConnection();
        bool opened = conn.State == System.Data.ConnectionState.Open;
        if (!opened) conn.Open();
        try
        {
            using var command = conn.CreateCommand();
            command.CommandText = "SELECT Value FROM TimeIdState WHERE Key = @key";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@key";
            parameter.Value = key;
            command.Parameters.Add(parameter);
            return command.ExecuteScalar() as string;
        }
        finally { if (!opened) conn.Close(); }
    }

    private static void WriteState(TimeIdDbContext db, string key, string value)
    {
        db.Database.ExecuteSqlInterpolated(
            $"INSERT INTO TimeIdState (Key, Value) VALUES ({key}, {value}) ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value");
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
