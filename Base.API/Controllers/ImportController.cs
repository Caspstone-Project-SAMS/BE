using Base.API.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImportController : ControllerBase
{
    private readonly ImportService _importService;
    public ImportController(ImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("schedules")]
    public async Task<IActionResult> ImportSchedules([FromForm] ImportSchedule resource)
    {
        if (ModelState.IsValid && resource.Image is not null)
        {
            var result = await _importService.ImportScheduleUsingImage(resource.Image, resource.UserId);
            return Ok(new
            {
                Title = "Import successfully",
                Result = result,
            });
        }
        return BadRequest(new
        {
            Title = "Import failed",
            Errors = new string[1] { "Invalid input" }
        });
    }
}

public class ImportSchedule
{
    [Required]
    public IFormFile? Image { get; set; }
    [Required]
    public Guid UserId { get; set; }
}
