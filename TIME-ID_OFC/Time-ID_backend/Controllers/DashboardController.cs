using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService service;
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public DashboardController(DashboardService service) => this.service = service;

    [HttpGet]
    public ActionResult<DashboardResponse> GetMetrics() => Ok(service.GetMetrics(ActorId));
}
