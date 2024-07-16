using AutoMapper;
using Base.API.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Service;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IMapper _mapper;
        private readonly WebSocketConnectionManager _webSocketConnectionManager;
        public AttendanceController(IAttendanceService attendanceService, IMapper mapper, WebSocketConnectionManager webSocketConnectionManager)
        {
            _attendanceService = attendanceService;
            _webSocketConnectionManager = webSocketConnectionManager;
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
        public async Task<IActionResult> UpdateAttendanceStatus([FromQuery]  int scheduleID, [FromQuery] int attendanceStatus, [FromQuery] DateTime? attendanceTime, [FromQuery] Guid studentID)
        {
            if (ModelState.IsValid)
            {
                var result = await _attendanceService.UpdateAttendanceStatus(scheduleID, attendanceStatus, attendanceTime, studentID);
                if (result.IsSuccess)
                {

                    // Make a real-time update using websocket here
                    var dataSend = new DataSend
                    {
                        studentID = studentID.ToString(),
                        status = 1
                    };
                    var dataSendString = JsonSerializer.Serialize(dataSend);
                    var messageSend = new MessageSend
                    {
                        Event = "statusChange",
                        Data = dataSendString
                    };
                    var messageSendString = JsonSerializer.Serialize(messageSend);
                    _webSocketConnectionManager.SendMessagesToAll(messageSendString);
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
        [HttpPut("update-list-student-status")]
        public async Task<IActionResult> UpdateListStudentStatus([FromBody] StudentListUpdateVM[] studentArr)
        {
            var result = await _attendanceService.UpdateListStudentStatus(studentArr);
            if (result.IsSuccess)
            {
                // real-time websocket
                foreach (var item in studentArr)
                {
                    var dataSend = new DataSend
                    {
                        studentID = item.StudentID.ToString() ?? "",
                        status = 1
                    };
                    var dataSendString = JsonSerializer.Serialize(dataSend);
                    var messageSend = new MessageSend
                    {
                        Event = "statusChange",
                        Data = dataSendString
                    };
                    var messageSendString = JsonSerializer.Serialize(messageSend);
                    _webSocketConnectionManager.SendMessagesToAll(messageSendString);
                }

                return Ok(new
                {
                    Title = result.Title,
                });
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            if(ModelState.IsValid && id > 0)
            {
                var attedance = await _attendanceService.GetAttendanceById(id);
                if(attedance is null)
                {
                    return NotFound(new
                    {
                        Title = "Attendance not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<AttendanceResponseVM>(attedance)
                });
            }

            return BadRequest(new
            {
                Title = "Get attendance information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
