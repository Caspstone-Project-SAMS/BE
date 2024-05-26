using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;

namespace Base.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private static IList<FingerprintTemplate> fingerprintTemplates = new List<FingerprintTemplate>();
    [HttpGet]
    public IActionResult Hello()
    {
        return Ok("Hello");
    }


    [HttpPost("fingerprint")]
    public IActionResult AddNew([FromBody] FingerprintTemplateTest fingerprintTemplate)
    {
        int largestId = 0;
        if (fingerprintTemplates.Count >= 1)
        {
            largestId = fingerprintTemplates.Select(f => f.Id).Max();
        }
        fingerprintTemplates.Add(new FingerprintTemplate
        {
            Id = largestId + 1,
            Fingerprint = fingerprintTemplate.fingerprintTemplate,
            IsAuthenticated = false
        });
        return Ok("Ok");
    }

    [HttpGet("fingerprint")]
    public IActionResult GetAllFingerprintTemplates()
    {
        return Ok(fingerprintTemplates.Select(f => new 
        { 
            Id = f.Id,
            Finger = f.Fingerprint 
        }));
    }

    [HttpGet("get-all-information")]
    public IActionResult GetALlInformation()
    {
        return Ok(fingerprintTemplates);
    }

    [HttpDelete("fingerprint")]
    public IActionResult DeleteAll()
    {
        fingerprintTemplates = new List<FingerprintTemplate>();
        return Ok("Delete all");
    }

    [HttpPut("attendance/{id}")]
    public IActionResult Attendance(int id)
    {
        var fingerprint = fingerprintTemplates.Where(f => f.Id == id).FirstOrDefault();
        if(fingerprint is null)
        {
            return NotFound("Fingerprint Id not found");
        }
        fingerprint.IsAuthenticated = true;
        return Ok("Attendance");
    }

    public class FingerprintTemplateTest
    {
        public string fingerprintTemplate { get; set; } = string.Empty;
    }

    public class FingerprintTemplate
    {
        public int Id { get; set; }
        public string Fingerprint { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; } = false;
    }
}