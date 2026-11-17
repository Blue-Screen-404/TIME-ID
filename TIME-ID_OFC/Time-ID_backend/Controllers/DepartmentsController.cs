using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeId.Operations;

[ApiController]
[Authorize]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService service;
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public DepartmentsController(DepartmentService service) => this.service = service;

    [HttpGet]
    public ActionResult<List<DepartmentOption>> GetAll() => Ok(service.GetAll());

    [HttpPost]
    public IActionResult Create(Team input) => Ok(service.Save(ActorId, input, null));

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, Team input) => Ok(service.Save(ActorId, input, id));

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        service.Delete(ActorId, id);
        return Ok(new { deleted = true });
    }
}
