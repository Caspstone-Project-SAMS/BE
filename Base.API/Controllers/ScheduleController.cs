using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly IMapper _mapper;

        public ScheduleController(IScheduleService scheduleService, IMapper mapper)
        {
            _scheduleService = scheduleService;
            _mapper = mapper;
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


        [HttpGet("test-get-all")]
        public async Task<IActionResult> GetALlSchedules(
            [FromQuery] int startPage,
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] Guid? lecturerId,
            [FromQuery] int? semesterId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            if (ModelState.IsValid)
            {
                DateOnly? startDateOnly = null;
                DateOnly? endDateOnly = null;
                if (startDate is not null)
                {
                    startDateOnly = DateOnly.FromDateTime(startDate.Value);
                }
                if(endDate is not null)
                {
                    endDateOnly = DateOnly.FromDateTime(endDate.Value);
                }
                var result = await _scheduleService.GetAllSchedules(startPage, endPage, quantity, lecturerId, semesterId, startDateOnly, endDateOnly);
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
                    Title = "Get schedules falied",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get schedules failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
