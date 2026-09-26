using Microsoft.AspNetCore.Identity;

public class AuthenticationService
{
    private readonly PasswordHasher<User> hasher = new();
    private readonly List<User> users = new();
    private readonly object gate = new();
    public string SessionStamp { get; } = Guid.NewGuid().ToString();

    public AuthenticationService()
    {
        AddDemoUser("Usuario de teste", "teste@timeid.local", "Administrador");
        AddDemoUser("Usuario comum", "usuario@timeid.local", "Usuário");
    }

    private void AddDemoUser(string name, string email, string role)
    {
        var user = new User { Id = Guid.NewGuid(), Username = name, Email = email, PasswordHash = "" };
        user.PasswordHash = hasher.HashPassword(user, "TimeId@123");
        user.AssignRole(new Role { Id = Guid.NewGuid(), Name = role });
        user.Activate();
        users.Add(user);
    }

    private static LoginResponse ToResponse(User user) =>
        new(user.Id, user.Username, user.Email, user.Role?.Name ?? "Usuário", user.PhotoUrl);

    public LoginResponse? ValidateCredentials(string email, string password)
    {
        lock (gate)
        {
            var user = users.FirstOrDefault(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
            // Executa o hash mesmo para um e-mail desconhecido.
            var candidate = user ?? users[0];
            var result = hasher.VerifyHashedPassword(candidate, candidate.PasswordHash, password);
            return user is { IsActive: true } && result != PasswordVerificationResult.Failed ? ToResponse(user) : null;
        }
    }

    public LoginResponse? FindActiveUser(Guid id)
    {
        lock (gate)
        {
            var user = users.FirstOrDefault(u => u.Id == id && u.IsActive);
            return user is null ? null : ToResponse(user);
        }
    }

    public LoginResponse? UpdateProfile(Guid id, UpdateProfileRequest request)
    {
        lock (gate)
        {
            var user = users.FirstOrDefault(u => u.Id == id && u.IsActive);
            if (user is null) return null;
            bool admin = user.Role?.Name == "Administrador";
            if (!admin && (request.Username is not null || request.Email is not null))
                throw new UnauthorizedAccessException("Seu perfil permite alterar apenas a foto.");
            string name = request.Username?.Trim() ?? user.Username;
            string email = request.Email?.Trim() ?? user.Email;
            if (name.Length < 2 || name.Length > 100)
                throw new ArgumentException("O nome deve ter entre 2 e 100 caracteres.");
            if (!System.Net.Mail.MailAddress.TryCreate(email, out var address) || address.Address != email)
                throw new ArgumentException("Informe um e-mail válido.");
            if (users.Any(u => u.Id != id && string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Este e-mail já está em uso.");
            ValidatePhoto(request.PhotoUrl);
            // Só altera o perfil depois de validar todos os campos.
            user.Username = name;
            user.UpdateEmail(email);
            user.PhotoUrl = request.PhotoUrl;
            return ToResponse(user);
        }
    }

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
