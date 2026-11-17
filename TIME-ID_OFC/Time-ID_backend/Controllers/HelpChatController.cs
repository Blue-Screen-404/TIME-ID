using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeId.Services;

[ApiController, Authorize, Route("api/help-chat")]
public sealed class HelpChatController(HelpChatService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<HelpChatResponse>> Ask(HelpChatRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await service.AskAsync(request, cancellationToken)); }
        catch (ArgumentException error) { return BadRequest(new { title = error.Message }); }
    }
}
