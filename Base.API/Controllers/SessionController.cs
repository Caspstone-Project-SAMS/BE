using Base.API.Service;
using Base.Service.Common;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SessionController : ControllerBase
{
    private readonly SessionManager _sessionManager;
    private readonly ICurrentUserService _currentUserService;

    public SessionController(SessionManager sessionManager, ICurrentUserService currentUserService)
    {
        _sessionManager = sessionManager;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id}")]
    public IActionResult GetSessionById(int id)
    {
        var session = _sessionManager.GetSessionById(id);
        if(session is null)
        {
            return NotFound();
        }
        return Ok(session);
    }

    [HttpGet]
    public IActionResult GetSessions([FromQuery] Guid? userId, 
        [FromQuery] int? category, 
        [FromQuery] int? state,
        [FromQuery] int? moduleId, 
        [FromQuery] Guid? studentId)
    {
        return Ok(_sessionManager.GetSessions(userId, state, category, moduleId, studentId));
    }

    [Authorize(Policy = "Admin Lecturer")]
    [HttpPost]
    public async Task<IActionResult> SubmitSession(int sessionId, [FromBody] FingerprintDescription? fingerprintDescription)
    {
        Guid userId = new Guid();
        var checkUserId =  Guid.TryParse(_currentUserService.UserId, out userId);
        if (!checkUserId)
        {
            return BadRequest(new
            {
                Title = "Submit session failed",
                Errors = new string[1] { "Invalid user information" }
            });
        }
        var result = await _sessionManager.SubmitSession(sessionId, userId, fingerprintDescription);
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }


    [HttpPost("schedule-preparation/state-update")]
    public IActionResult UpdateSchedulePreparationState([FromBody] SchedulePreparationState resource)
    {
        if (ModelState.IsValid)
        {
            var result = _sessionManager.UpdateSchedulePreparationResult(resource.SessionId, resource.UploadedFingerprints, resource.ScheduleId);
            if (result)
            {
                return Ok(new
                {
                    Title = "Update state successfully"
                });
            }
            return BadRequest(new
            {
                Title = "Update state failed"
            });
        }
        return BadRequest(new
        {
            Title = "Update state failed",
            Errors = new string[1] { "Invalid input" }
        });
    }
}

public class SchedulePreparationState
{
    public int SessionId { get; set; }
    public int UploadedFingerprints { get; set; }
    public int ScheduleId { get; set; }
}

public class FingerprintDescription
{
    public string? fingerprint1Description { get; set; }
    public string? fingerprint2Description { get; set; }
}
