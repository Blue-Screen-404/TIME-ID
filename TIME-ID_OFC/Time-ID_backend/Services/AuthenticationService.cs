using Microsoft.AspNetCore.Identity;

public class AuthenticationService
{
    private readonly PasswordHasher<User> hasher = new();
    private readonly User demoUser;

    // Muda ao reiniciar o backend, invalidando cookies da execução anterior.
    public string SessionStamp { get; } = Guid.NewGuid().ToString();

    public AuthenticationService()
    {
        demoUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "Usuario de teste",
            Email = "teste@timeid.local",
            PasswordHash = string.Empty
        };
        // Credencial pública de demonstração, somente para testes locais.
        demoUser.PasswordHash = hasher.HashPassword(demoUser, "TimeId@123");
        demoUser.Activate();
    }

    public LoginResponse? ValidateCredentials(string email, string password)
    {
        // Verifica o hash mesmo se o e-mail não existir.
        var passwordResult = hasher.VerifyHashedPassword(demoUser, demoUser.PasswordHash, password);
        if (!demoUser.IsActive ||
            !string.Equals(demoUser.Email, email.Trim(), StringComparison.OrdinalIgnoreCase) ||
            passwordResult == PasswordVerificationResult.Failed)
            return null;

        return FindActiveUser(demoUser.Id);
    }

    public LoginResponse? FindActiveUser(Guid id) =>
        demoUser.IsActive && demoUser.Id == id
            ? new LoginResponse(demoUser.Id, demoUser.Username, demoUser.Email)
            : null;
}
