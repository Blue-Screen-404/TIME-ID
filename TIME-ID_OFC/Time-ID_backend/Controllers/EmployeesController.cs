using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService service;
    public EmployeesController(EmployeeService service) => this.service = service;

    [HttpGet]
    public ActionResult<List<EmployeeResponse>> GetAll([FromQuery] string? search, [FromQuery] EmployeeStatus? status)
        => Ok(service.GetAll(search, status));

    [HttpGet("{id:guid}")]
    public ActionResult<EmployeeResponse> GetById(Guid id)
        => service.GetById(id) is { } employee ? Ok(employee) : NotFound();

    [HttpPost]
    public ActionResult<EmployeeResponse> Create(CreateEmployeeRequest request)
    {
        try
        {
            var employee = service.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public ActionResult<EmployeeResponse> Update(Guid id, UpdateEmployeeRequest request)
    {
        try
        {
            var employee = service.Update(id, request);
            return employee is null ? NotFound() : Ok(employee);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPatch("{id:guid}/deactivate")]
    public IActionResult Deactivate(Guid id) => service.Deactivate(id) ? NoContent() : NotFound();
}
