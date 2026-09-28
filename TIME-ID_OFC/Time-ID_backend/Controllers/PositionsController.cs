using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly PositionService service;

    public PositionsController(PositionService service) => this.service = service;

    [HttpGet]
    public ActionResult<List<PositionOption>> GetAll() => Ok(service.GetAll());
}
