using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImportSchedulesRecordController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IImportSchedulesRecordService _importSchedulesRecordService;

    public ImportSchedulesRecordController(IMapper mapper, IImportSchedulesRecordService importSchedulesRecordService)
    {
        _mapper = mapper;
        _importSchedulesRecordService = importSchedulesRecordService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllImportRecords(
        [FromQuery] int startPage,
        [FromQuery] int endPage,
        [FromQuery] int quantity,
        [FromQuery] Guid? userId)
    {
        if (ModelState.IsValid)
        {
            var result = await _importSchedulesRecordService.GetAllRecord(startPage, endPage, quantity, userId);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title,
                    Result = _mapper.Map<IEnumerable<ImportSchedulesRecordResponseVM>>(result.Result)
                });
            }
            return BadRequest(new
            {
                Title = "Get records failed",
                Errors = result.Errors
            });
        }
        return BadRequest(new
        {
            Title = "Get import records failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [HttpPost("revert/{id}")]
    public async Task<IActionResult> RevertImportRecord(int id)
    {
        if(id > 0)
        {
            var result = await _importSchedulesRecordService.RevertRecords(id);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title
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
            Title = "Revert record failed",
            Errors = new string[1] { "Invalid input" }
        });
    }
}
