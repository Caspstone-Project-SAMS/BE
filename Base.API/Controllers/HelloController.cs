using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private static List<string> fingerprintTemplates = new List<string>();
    [HttpGet]
    public IActionResult Hello()
    {
        return Ok("Hello");
    }


    [HttpPost("fingerprint")]
    public IActionResult AddNew([FromBody] FingerprintTemplateTest fingerprintTemplate)
    {
        fingerprintTemplates.Add(fingerprintTemplate.fingerprintTemplate);
        return Ok("Ok");
    }

    [HttpGet("fingerprint")]
    public IActionResult GetAllFingerprintTemplates()
    {
        return Ok(fingerprintTemplates);
    }

    [HttpDelete("fingerprint")]
    public IActionResult DeleteAll()
    {
        fingerprintTemplates = new List<string>();
        return Ok("Delete all");
    }

    public class FingerprintTemplateTest
    {
        public string fingerprintTemplate { get; set; } = string.Empty;
    }
}