using AutoMapper;
using Base.Service.IService;
using Base.Service.Service;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IMapper _mapper;
        public AttendanceController(IAttendanceService attendanceService, IMapper mapper)
        {
            _attendanceService = attendanceService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAttendance([FromQuery]int startPage, [FromQuery] int endPage, [FromQuery] int? quantity, [FromQuery] int scheduleID)
        {
            if (ModelState.IsValid)
            {
                var attendances =  await _attendanceService.GetAttendances(startPage, endPage, quantity, scheduleID);
                return Ok(_mapper.Map<IEnumerable<AttendanceResponse>>(attendances));
            }

            return BadRequest(new
            {
                Title = "Get Students information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPut("update-attendance-status")]
        public async Task<IActionResult> UpdateAttendanceStatus([FromQuery] int attendanceID, [FromQuery] int attendanceStatus, [FromQuery] DateTime? attendanceTime, [FromQuery] Guid studentID)
        {
            if (ModelState.IsValid)
            {
                var result = await _attendanceService.UpdateAttendanceStatus(attendanceID, attendanceStatus, attendanceTime, studentID);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                    });
                }

                return BadRequest(result);
            }

            return BadRequest(new
            {
                Title = "Update Status failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
