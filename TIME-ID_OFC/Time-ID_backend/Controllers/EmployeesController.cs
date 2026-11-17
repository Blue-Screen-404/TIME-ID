using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Authorize]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService service;
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public EmployeesController(EmployeeService service) => this.service = service;

    [HttpGet]
    public ActionResult<List<EmployeeResponse>> GetAll(
        [FromQuery] string? search,
        [FromQuery] EmployeeStatus? status) => Ok(service.GetAll(ActorId, search, status));

    [HttpGet("{id:guid}")]
    public ActionResult<EmployeeResponse> GetById(Guid id) =>
        service.GetById(ActorId, id) is { } employee ? Ok(employee) : NotFound();

    [HttpPost]
    public ActionResult<EmployeeResponse> Create(CreateEmployeeRequest request)
    {
        try
        {
            EmployeeResponse employee = service.Create(ActorId, request);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public ActionResult<EmployeeResponse> Update(Guid id, UpdateEmployeeRequest request)
    {
        try
        {
            EmployeeResponse? employee = service.Update(ActorId, id, request);
            return employee is null ? NotFound() : Ok(employee);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPatch("{id:guid}/deactivate")]
    public IActionResult Deactivate(Guid id) =>
        service.Deactivate(ActorId, id) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        service.Delete(ActorId, id);
        return NoContent();
    }
}
