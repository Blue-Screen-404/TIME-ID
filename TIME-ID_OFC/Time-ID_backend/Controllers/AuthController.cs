using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService service;

    public AuthController(AuthenticationService service)
    {
        this.service = service;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        LoginResponse? user = service.ValidateCredentials(request.Email, request.Password);
        if (user == null)
        {
            return Problem(statusCode: 401, title: "E-mail ou senha inválidos.");
        }

        ClaimsIdentity identity = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("session_stamp", service.SessionStamp)
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });
        return Ok(user);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<LoginResponse> Me()
    {
        Guid id;
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id))
        {
            return Unauthorized();
        }

        LoginResponse? user = service.FindActiveUser(id);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [Authorize]
    [HttpPut("profile")]
    [RequestSizeLimit(2900000)]
    public ActionResult<LoginResponse> UpdateProfile(UpdateProfileRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        try
        {
            var profile = service.UpdateProfile(id, request);
            return profile is null ? Unauthorized() : Ok(profile);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Problem(statusCode: 403, title: exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: 400, title: exception.Message);
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
