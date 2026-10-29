using TimeId.Operations;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        // Aplicação local: registra os diagnósticos no terminal, sem exigir acesso ao Event Log do Windows.
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.AddControllers();
        // A conexão é configurada para uso futuro; SQLite só cria o arquivo quando
        // alguma operação abre a conexão ou executa uma migração explicitamente.
        string sqliteConnection = builder.Configuration.GetConnectionString("TimeId")
            ?? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "App_Data", "timeid.sqlite")}";
        builder.Services.AddDbContext<TimeIdDbContext>(options => options.UseSqlite(sqliteConnection));
        // Cookies continuam invalidados por session stamp; chave local também protege os
        // descritores faciais cifrados que o administrador cadastrará no futuro.
        IDataProtectionBuilder dataProtection = builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(
            Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")));
        if (OperatingSystem.IsWindows()) dataProtection.ProtectKeysWithDpapi();
        builder.Services.AddSingleton<DataStore>();
        builder.Services.AddSingleton<PasswordResetEmailService>();
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<EmployeeService>();
        builder.Services.AddSingleton<DepartmentService>();
        builder.Services.AddSingleton<PositionService>();
        builder.Services.AddSingleton<DashboardService>();
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
                    if (context.Request.Path.StartsWithSegments("/api"))
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    else
                        context.Response.Redirect("/login");
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnValidatePrincipal = async context =>
                {
                    AuthenticationService service = context.HttpContext.RequestServices.GetRequiredService<AuthenticationService>();
                    if (context.Principal?.FindFirstValue("session_stamp") != service.SessionStamp ||
                        !Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ||
                        service.FindActiveUser(id) is null ||
                        context.Principal?.FindFirstValue("security_stamp") != service.SecurityStamp(id))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                };
            });
        builder.Services.AddAuthorization();

        WebApplication app = builder.Build();
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api")) context.Response.Headers.CacheControl = "no-store";
            try { await next(context); }
            catch (DomainError error)
            {
                context.Response.StatusCode = error.Status;
                await context.Response.WriteAsJsonAsync(new { title = error.Message, status = error.Status });
            }
        });
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/api", () => Results.Ok(new
        {
            application = "TIME-ID API",
            mode = "Gestão local com persistência em arquivo",
            login = "/api/auth/login",
            me = "/api/auth/me",
            logout = "/api/auth/logout"
        }));
        app.MapControllers();
        // Somente rotas da interface: URLs desconhecidas da API continuam retornando 404.
        app.MapGet("/", () => Results.Redirect("/login"));
        app.MapGet("/login", (HttpContext context) =>
            context.User.Identity?.IsAuthenticated == true
                ? Results.Redirect("/inicio")
                : Results.File(Path.Combine(app.Environment.WebRootPath, "index.html"), "text/html"));
        app.MapGet("/terminal", () => Results.File(Path.Combine(app.Environment.WebRootPath, "index.html"), "text/html"));
        foreach (string path in new[] { "/inicio", "/cadastro", "/funcionarios", "/listar", "/departamentos", "/ponto", "/facial", "/ferias", "/feriados", "/relatorios", "/configuracoes" })
        {
            app.MapGet(path, () => Results.File(Path.Combine(app.Environment.WebRootPath, "index.html"), "text/html"))
                .RequireAuthorization();
        }
        app.Run();
    }
}
