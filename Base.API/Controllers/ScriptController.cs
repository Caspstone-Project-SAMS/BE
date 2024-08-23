using Base.API.Service;
using Base.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScriptController : ControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly WebSocketConnectionManager1 _websocketConnectionManager;
    public ScriptController(IScriptService scriptService, WebSocketConnectionManager1 webSocketConnectionManager)
    {
        _scriptService = scriptService;
        _websocketConnectionManager = webSocketConnectionManager;
    }

    [Authorize(Policy = "Admin")]
    [HttpPost("set-time")]
    public IActionResult SetServerTime([FromQuery] DateTime resource)
    {
        if (ModelState.IsValid)
        {
            _scriptService.SetServerTime(resource);
            var messageSend = new WebsocketMessage
            {
                Event = "SetupDateTime",
                Data = new
                {
                    UpdatedDateTime = resource.ToString("yyyy-MM-dd HH:mm:ss")
                }
            };
            var jsonPayload = JsonSerializer.Serialize(messageSend);
            _ = _websocketConnectionManager.SendMessageToAllModule(jsonPayload);
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
    [HttpPost("reset-time")]
    public IActionResult ResetServerTime()
    {
        _scriptService.ResetServerTime();
        return Ok(new
        {
            Title = "Reset server time successfully"
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
