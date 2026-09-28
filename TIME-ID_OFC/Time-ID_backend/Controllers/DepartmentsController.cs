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
    public ActionResult<List<DepartmentOption>> GetAll() => Ok(service.GetAll());
}
