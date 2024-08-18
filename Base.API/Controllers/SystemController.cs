using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    private readonly ISystemService _systemService;
    public SystemController(ISystemService systemService)
    {
        _systemService = systemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetConfigurations()
    {
        var result = await _systemService.GetSystemConfiguration();
        return Ok(new
        {
            Title = "Get successfully",
            Result = result
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateConfiguration(SystemConfigurationVM resource)
    {
        if (ModelState.IsValid)
        {
            var result = await _systemService.UpdateConfiguration(resource);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title,
                    Result = result.Result
                });
            }
            return BadRequest(new
            {
                Title = result.Title,
                Errors = result.Errors
            });
        }
        return BadRequest(new
        {
            Title = "Update system configuration failed",
            Errors = new string[1] { "Invalid input" }
        });
    }
}
