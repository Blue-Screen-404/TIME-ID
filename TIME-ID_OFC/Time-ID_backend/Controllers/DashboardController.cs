using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService service;
    public DashboardController(DashboardService service) => this.service = service;

    [HttpGet]
    public ActionResult<DashboardResponse> GetMetrics() => Ok(service.GetMetrics());
}
