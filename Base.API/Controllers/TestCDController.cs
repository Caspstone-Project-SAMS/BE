using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCDController : ControllerBase
    {
        [HttpGet]
        public IActionResult TestCD()
        {
            return Ok();
        }
    }
}
