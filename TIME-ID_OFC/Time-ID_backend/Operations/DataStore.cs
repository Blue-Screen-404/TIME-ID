using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace TimeId.Operations;

// Armazenamento local de uma única instância. As mutações são validadas em uma cópia
// e gravadas por substituição atômica antes de publicar o novo estado em memória.
public class DataStore
{
    private readonly object gate = new();
    private readonly string path;
    private readonly JsonSerializerOptions json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private Database data;
    public DataStore(IWebHostEnvironment environment, IConfiguration configuration)
    {
        path = Path.GetFullPath(configuration["TIMEID_DATA_PATH"] ?? Path.Combine(environment.ContentRootPath, "App_Data", "timeid.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (File.Exists(path))
        {
            data = JsonSerializer.Deserialize<Database>(File.ReadAllText(path), json) ?? throw new InvalidDataException("Dados locais inválidos.");
            if (data.Version != 1) throw new InvalidDataException("Versão de dados não suportada.");
        }
        else
        {
            data = new Database();
            var hasher = new PasswordHasher<Account>();
            foreach (var (name, email, role) in new[] { ("Usuario de teste", "teste@timeid.local", "Administrador"), ("Usuario comum", "usuario@timeid.local", "Usuário") })
            {
                var account = new Account { Username = name, Email = email, Role = role };
                account.PasswordHash = hasher.HashPassword(account, "TimeId@123");
                data.Accounts.Add(account);
            }
            Persist(data);
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
    private void Persist(Database next)
    {
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(next, json));
        File.Move(temp, path, true);
    }
    public static Account Actor(Database db, Guid id) => db.Accounts.FirstOrDefault(a => a.Id == id && a.Active) ?? throw new DomainError(401, "Sessão inválida.");
    public static void Admin(Account actor) { if (actor.Role != "Administrador") throw new DomainError(403, "Somente administradores podem executar esta ação."); }
    public static void Log(Database db, Account actor, string action, string entity, string detail) =>
        db.Audit.Add(new(Guid.NewGuid(), DateTimeOffset.UtcNow, actor.Id, actor.Username, action, entity, detail));
    public static DateOnly Today => Date(DateTimeOffset.UtcNow);
    public static DateOnly Date(DateTimeOffset at) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(at, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")).DateTime);
}
