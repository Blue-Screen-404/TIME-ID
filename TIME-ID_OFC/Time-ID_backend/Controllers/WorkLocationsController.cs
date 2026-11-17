using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeId.DTOs.Geolocation;
using TimeId.Services;
namespace TimeId.Controllers;

[ApiController, Authorize, Route("api/work-locations")]
public sealed class WorkLocationsController(WorkLocationService service, GeolocationValidationService validation) : ControllerBase
{
    private Guid ActorId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet]
    public IActionResult List() => Ok(service.List(ActorId));
    [HttpPost]
    public IActionResult Create(CreateWorkLocationRequest input)
    {
        var result = service.Save(ActorId, null, input);
        return StatusCode(StatusCodes.Status201Created, result);
    }
    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, UpdateWorkLocationRequest input) => Ok(service.Save(ActorId, id, input));
    [HttpPost("{id:guid}/deactivate")]
    public IActionResult Deactivate(Guid id) => Ok(service.Deactivate(ActorId, id));
    [HttpPost("validate/{employeeId:guid}")]
    public IActionResult Validate(Guid employeeId, PunchLocationInput input) => Ok(validation.Validate(ActorId, employeeId, input));
}
