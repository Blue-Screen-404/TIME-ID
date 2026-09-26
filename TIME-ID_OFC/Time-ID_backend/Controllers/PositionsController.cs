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
    public ActionResult<List<PositionResponse>> GetAll() => Ok(service.GetAll());

    [HttpGet("{id:guid}")]
    public ActionResult<PositionResponse> GetById(Guid id)
        => service.GetById(id) is { } item ? Ok(item) : NotFound();

    [HttpPost]
    public ActionResult<PositionResponse> Create(PositionRequest request)
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
    public ActionResult<PositionResponse> Update(Guid id, PositionRequest request)
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
