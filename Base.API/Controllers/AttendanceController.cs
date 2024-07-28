using AutoMapper;
using Base.API.Service;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Service;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using Duende.IdentityServer.Events;
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
        private readonly WebSocketConnectionManager1 _webSocketConnectionManager;
        private readonly IScheduleService _scheduleService;
        public AttendanceController(IAttendanceService attendanceService, 
            IMapper mapper, 
            WebSocketConnectionManager1 webSocketConnectionManager,
            IScheduleService scheduleService)
        {
            _attendanceService = attendanceService;
            _webSocketConnectionManager = webSocketConnectionManager;
            _mapper = mapper;
            _scheduleService = scheduleService;
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
                    _ = UpdateAttendanceStatus(scheduleID, studentID);

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
                /*// real-time websocket
                foreach (var item in studentArr)
                {
                    var dataSend = new DataSend
                    {
                        studentID = item.StudentID.ToString() ?? "",
                        status = 1
                    };
                    var dataSendString = JsonSerializer.Serialize(dataSend);
                    var messageSend = new WebsocketMessage
                    {
                        Event = "statusChange",
                        Data = dataSendString
                    };
                    var messageSendString = JsonSerializer.Serialize(messageSend);
                    _webSocketConnectionManager.SendMessagesToAll(messageSendString);
                }*/
                var newobject = new
                {
                    Event = "StudentAttended",
                    Data = new
                    {
                        studentIDs = new List<string>() { "aaaa", "aaaa" }
                    }
                };

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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAttendances(
            [FromQuery] int startPage, 
            [FromQuery] int endPage, 
            [FromQuery] int quantity,
            [FromQuery] int? attendanceStatus,
            [FromQuery] int? scheduleID,
            [FromQuery] Guid? studentId,
            [FromQuery] int? classId)
        {
            if (ModelState.IsValid)
            {
                var result = await _attendanceService.GetAttendanceList(startPage, endPage, quantity, attendanceStatus, scheduleID, studentId, classId);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<IEnumerable<AttendancesResponseVM>>(result.Result)
                    });
                }
                return BadRequest(new
                {
                    Title = "Get attendances falied",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get attendances failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        private async Task UpdateAttendanceStatus(int scheduleID, Guid studentID)
        {
            var existedClass = (await _scheduleService.GetById(scheduleID))?.Class;
            if (existedClass is null) return;
            var messageSend = new WebsocketMessage
            {
                Event = "StudentAttended",
                Data = new
                {
                    studentIDs = new List<string>() { studentID.ToString() },
                    scheduleID = scheduleID,
                }
            };
            var jsonPayload = JsonSerializer.Serialize(messageSend);
            await _webSocketConnectionManager.SendMessageToClient(jsonPayload, existedClass.LecturerID);
        }

        private async Task UpdateAttendancesStatus(int scheduleID, List<Guid> studentIDs)
        {
            var existedClass = (await _scheduleService.GetById(scheduleID))?.Class;
            if (existedClass is null) return;
            var messageSend = new WebsocketMessage
            {
                Event = "StudentAttended",
                Data = new
                {
                    studentIDs = new List<string>(studentIDs.Select(s => s.ToString())),
                    scheduleID = scheduleID,
                }
            };
            var jsonPayload = JsonSerializer.Serialize(messageSend);
            await _webSocketConnectionManager.SendMessageToClient(jsonPayload, existedClass.LecturerID);
        }
    }
}
