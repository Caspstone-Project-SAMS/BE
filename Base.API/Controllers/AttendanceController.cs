using AutoMapper;
using Base.API.Service;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using ClosedXML.Excel;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public AttendanceController(IAttendanceService attendanceService, 
            IMapper mapper, 
            WebSocketConnectionManager1 webSocketConnectionManager,
            IScheduleService scheduleService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _attendanceService = attendanceService;
            _webSocketConnectionManager = webSocketConnectionManager;
            _mapper = mapper;
            _scheduleService = scheduleService;
            _serviceScopeFactory = serviceScopeFactory;
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
                _ = UpdateAttendancesStatus(studentArr);

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
            using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
            var scheduleService = serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();

            var existedSchedule = await scheduleService.GetById(scheduleID);
            var existedClass = existedSchedule?.Class;
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

        private async Task UpdateAttendancesStatus(StudentListUpdateVM[] studentArr)
        {
            var scheduleID = studentArr.FirstOrDefault()?.ScheduleID ?? 0;
            var studentIDs = studentArr.Where(s => s.StudentID != null).Select(s => s.StudentID);

            using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
            var scheduleService = serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();

            var existedSchedule = await scheduleService.GetById(scheduleID);
            var existedClass = existedSchedule?.Class;
            if (existedClass is null) return;
            var messageSend = new WebsocketMessage
            {
                Event = "StudentAttended",
                Data = new
                {
                    studentIDs = new List<string?>(studentIDs.Select(s => s.ToString()) ?? Enumerable.Empty<string>()),
                    scheduleID = scheduleID,
                }
            };
            var jsonPayload = JsonSerializer.Serialize(messageSend);
            await _webSocketConnectionManager.SendMessageToClient(jsonPayload, existedClass.LecturerID);
        }


        [HttpGet("attendance-report")]

        public async Task<IActionResult> GetAttendanceReport(int classId,bool isExport = false)
        {
            try
            {
                var response = await _attendanceService.GetAttendanceReport(classId);
                if (isExport)
                {
                    if (response == null || !response.Any())
                    {
                        return BadRequest("No data available for export.");
                    }

                    // Create the workbook and worksheet
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Attendance Report");

                        // Set headers
                        worksheet.Cell("A1").Value = "Student Code";
                        worksheet.Cell("B1").Value = "Student Name";
                        worksheet.Cell("C1").Value = "Absence Percentage";
                        worksheet.Cell("D1").Value = "Date";
                        worksheet.Cell("E1").Value = "Status";

                        int row = 2;

                        // Populate the worksheet with data from the response
                        foreach (var student in response)
                        {
                            if (student.AttendanceRecords != null)
                            {
                                foreach (var record in student.AttendanceRecords)
                                {
                                    worksheet.Cell(row, 1).Value = student.StudentCode;
                                    worksheet.Cell(row, 2).Value = student.StudentName;
                                    worksheet.Cell(row, 3).Value = student.AbsencePercentage;
                                    worksheet.Cell(row, 4).Value = record.Date.ToString("yyyy-MM-dd");
                                    worksheet.Cell(row, 5).Value = MapStatusToString(record.Status);

                                    row++;
                                }
                            }
                        }

                        // Save the workbook to a MemoryStream
                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);

                            // Reset the stream position to the beginning
                            stream.Position = 0;

                            // Return the file as a FileStreamResult
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
                        }
                    }
                }

                if (response != null)
                {
                    return Ok(response);
                }

                return BadRequest("Error retrieving attendance report.");
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging library here)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private string MapStatusToString(int status)
        {
            return status switch
            {
                1 => "P",
                2 => "A",
                _ => "-"
            };
        }

    }
}
