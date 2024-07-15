using AutoMapper;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ScheduleController(IScheduleService scheduleService, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _scheduleService = scheduleService;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ScheduleResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ScheduleResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> Get([FromQuery]int startPage, [FromQuery] int endPage, [FromQuery] Guid lecturerId, [FromQuery] int quantity, [FromQuery] int? semesterId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate) 
        { 
            if(ModelState.IsValid)
            {
                var schedule = await _scheduleService.GetSchedules(startPage,endPage, lecturerId,quantity, semesterId, startDate, endDate);
                var result = _mapper.Map<IEnumerable<ScheduleResponse>>(schedule);
                if(result.Count() <= 0)
                {
                    return NotFound("Lecturer not have any Schedule");
                }
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedules(List<ScheduleVM> resources)
        {
            try
            {
                var response = await _scheduleService.Create(resources);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("download-excel-template")]
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "template_schedule.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "template_schedule.xlsx");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            if (ModelState.IsValid && id > 0)
            {
                var existedSchedule = await _scheduleService.GetById(id);
                if (existedSchedule is null)
                {
                    return NotFound(new
                    {
                        Title = "Schedule not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<ScheduleResponseVM>(existedSchedule)
                });
            }
            return BadRequest(new
            {
                Title = "Get schedule information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
