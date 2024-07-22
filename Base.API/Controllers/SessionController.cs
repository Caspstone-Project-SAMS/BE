using Base.API.Service;
using Base.Service.Common;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
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
            [FromQuery] int category, 
            [FromQuery] int? state,
            [FromQuery] int? moduleId, 
            [FromQuery] Guid? studentId)
        {
            return Ok(_sessionManager.GetSessions(userId, state, category, moduleId, studentId));
        }

        [Authorize(Policy = "Admin Lecturer")]
        [HttpPost]
        public async Task<IActionResult> SubmitSession(int sessionId)
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
            var result = await _sessionManager.SubmitSession(sessionId, userId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
