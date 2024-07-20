using Base.API.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly SessionManager _sessionManager;
        public SessionController(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
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
        public IActionResult GetSessions([FromQuery] Guid? userId, [FromQuery] int category, [FromQuery] int? state)
        {
            return Ok(_sessionManager.GetSessions(userId, state, category));
        }
    }
}
