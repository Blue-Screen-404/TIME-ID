using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
// Aplicação local: registra os diagnósticos no terminal, sem exigir acesso ao Event Log do Windows.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
// Chaves temporárias: cookies deixam de valer ao reiniciar, sem gravar no perfil do Windows.
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<AuthenticationService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TimeId.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        // Permite HTTP em localhost; HTTPS gera cookie Secure.
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = false;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            var service = context.HttpContext.RequestServices.GetRequiredService<AuthenticationService>();
            if (context.Principal?.FindFirstValue("session_stamp") != service.SessionStamp ||
                !Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ||
                service.FindActiveUser(id) is null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new
{
    application = "TIME-ID API",
    mode = "Login temporário com usuário em memória",
    login = "/api/auth/login",
    me = "/api/auth/me",
    logout = "/api/auth/logout"
}));
app.MapControllers();
app.Run();
