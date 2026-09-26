using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService service;
    public DepartmentsController(DepartmentService service) => this.service = service;

    [HttpGet]
    public ActionResult<List<DepartmentResponse>> GetAll() => Ok(service.GetAll());

    [HttpGet("{id:guid}")]
    public ActionResult<DepartmentResponse> GetById(Guid id)
        => service.GetById(id) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public ActionResult<DepartmentResponse> Create(DepartmentRequest request)
    {
        try
        {
            var item = service.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public ActionResult<DepartmentResponse> Update(Guid id, DepartmentRequest request)
    {
        try
        {
            var item = service.Update(id, request);
            return item is null ? NotFound() : Ok(item);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        try { return service.Delete(id) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
