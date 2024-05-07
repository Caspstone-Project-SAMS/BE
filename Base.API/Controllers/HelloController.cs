using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Hello()
    {
        return Ok("Hello");
    }
}