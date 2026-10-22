using Microsoft.AspNetCore.Identity;
using TimeId.Operations;

public class AuthenticationService(DataStore store)
{
    private readonly PasswordHasher<Account> hasher = new();
    public string SessionStamp { get; } = Guid.NewGuid().ToString();
    private static LoginResponse ToResponse(Account user) => new(user.Id, user.Username, user.Email, user.Role, user.PhotoUrl);
    public string? SecurityStamp(Guid id) => store.Read(db => db.Accounts.FirstOrDefault(a => a.Id == id && a.Active)?.SecurityStamp);
    public LoginResponse? ValidateCredentials(string email, string password, string? code = null) => store.Write(db =>
    {
        var user = db.Accounts.FirstOrDefault(a => a.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        var candidate = user ?? db.Accounts[0];
        var result = hasher.VerifyHashedPassword(candidate, candidate.PasswordHash, password);
        if (user is not { Active: true } || result == PasswordVerificationResult.Failed || !TwoFactor.VerifyAccount(user, code)) return null;
        DataStore.Log(db, user, "Entrou", "Acesso", "Login realizado.");
        return ToResponse(user);
    });
    public LoginResponse? FindActiveUser(Guid id) => store.Read(db =>
    {
        var user = db.Accounts.FirstOrDefault(a => a.Id == id && a.Active);
        return user is null ? null : ToResponse(user);
    });
    public LoginResponse? UpdateProfile(Guid id, UpdateProfileRequest request) => store.Write(db =>
    {
        var user = db.Accounts.FirstOrDefault(a => a.Id == id && a.Active);
        if (user is null) return null;
        if (user.Role != "Administrador" && (request.Username is not null || request.Email is not null))
            throw new UnauthorizedAccessException("Seu perfil permite alterar apenas a foto.");
        string name = request.Username?.Trim() ?? user.Username;
        string email = request.Email?.Trim() ?? user.Email;
        if (name.Length < 2 || name.Length > 100) throw new ArgumentException("O nome deve ter entre 2 e 100 caracteres.");
        if (!System.Net.Mail.MailAddress.TryCreate(email, out var address) || address.Address != email) throw new ArgumentException("Informe um e-mail válido.");
        if (db.Accounts.Any(a => a.Id != id && a.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) || db.Employees.Any(e => e.Id != user.EmployeeId && e.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Este e-mail já está em uso.");
        ValidatePhoto(request.PhotoUrl);
        user.Username = name; user.Email = email; user.PhotoUrl = request.PhotoUrl;
        var employee = db.Employees.FirstOrDefault(e => e.Id == user.EmployeeId);
        if (employee is not null) { employee.Name = name; employee.Email = email; }
        DataStore.Log(db, user, "Editou", "Perfil", "Dados do próprio perfil atualizados.");
        return ToResponse(user);
    });

    private static void ValidatePhoto(string? photo)
    {
        if (photo is null) return;
        string[] prefixes = { "data:image/png;base64,", "data:image/jpeg;base64,", "data:image/webp;base64," };
        string? prefix = prefixes.FirstOrDefault(photo.StartsWith);
        if (prefix is null) throw new ArgumentException("Use uma imagem PNG, JPEG ou WebP.");
        byte[] bytes;
        try { bytes = Convert.FromBase64String(photo[prefix.Length..]); }
        catch (FormatException) { throw new ArgumentException("A imagem enviada é inválida."); }
        if (bytes.Length > 2 * 1024 * 1024 || bytes.Length < 12)
            throw new ArgumentException("A foto deve ter até 2 MB e ser uma imagem válida.");
        bool valid = prefix.Contains("png") ? bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137, 80, 78, 71, 13, 10, 26, 10})
            : prefix.Contains("jpeg") ? bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255
            : bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        if (!valid) throw new ArgumentException("O conteúdo da imagem não corresponde ao formato informado.");
    }
}
