using Base.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScriptController : ControllerBase
{
    private readonly IScriptService _scriptService;
    public ScriptController(IScriptService scriptService)
    {
        _scriptService = scriptService;
    }

    [Authorize(Policy = "Admin")]
    [HttpPost("set-time")]
    public IActionResult SetServerTime([FromQuery] DateTime resource)
    {
        if (ModelState.IsValid)
        {
            _scriptService.SetServerTime(resource);
            return Ok(new
            {
                Title = "Set server time successfully"
            });
        }
        return BadRequest(new
        {
            Title = "Set server time failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [Authorize(Policy = "Admin")]
    [HttpPost("register-fingerprint")]
    public async Task<IActionResult> AutoRegisterFingerprints()
    {
        await _scriptService.AutoRegisterFingerprint();
        return Ok();
    }
}
