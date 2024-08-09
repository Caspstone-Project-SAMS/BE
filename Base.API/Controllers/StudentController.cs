using Microsoft.AspNetCore.Mvc;
using Base.Service.ViewModel.RequestVM;
using Base.IService.IService;
using AutoMapper;
using Base.Service.ViewModel.ResponseVM;
using Base.API.Service;
using System.Text.Json;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;
        private readonly SessionManager _sessionManager;
        private readonly WebSocketConnectionManager1 _websocketConnectionManager;

        public StudentController(IStudentService studentService, IMapper mapper, SessionManager sessionManager, WebSocketConnectionManager1 webSocketConnectionManager)
        {
            _studentService = studentService;
            _mapper = mapper;
            _sessionManager = sessionManager;
            _websocketConnectionManager = webSocketConnectionManager;
        }
        

        [HttpGet]
        public async Task<IActionResult> GetAllStudents([FromQuery]int startPage, [FromQuery] int endPage, [FromQuery] int? quantity, [FromQuery] Guid? studentID, [FromQuery] string? studentCode, [FromQuery] bool isModule = false)
        {
            if (ModelState.IsValid)
            {
                var students = await _studentService.GetStudents(startPage, endPage, quantity, studentID,studentCode);
                if(isModule)
                {
                    return Ok(_mapper.Map<IEnumerable<StudentModuleResponse>>(students));
                }
                return Ok(_mapper.Map<IEnumerable<StudentResponse>>(students));
            }
            
            return BadRequest(new
            {
                Title = "Get Students information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewStudent(List<StudentVM> resources)
        {    
                var result = await _studentService.CreateStudent(resources);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
        }

        [HttpGet("get-students-by-classId")]
        public async Task<IActionResult> GetAllStudents([FromQuery] int classID, [FromQuery] int startPage, [FromQuery] int endPage, [FromQuery] int? quantity, [FromQuery] int? sessionId, [FromQuery] Guid? userId, [FromQuery] bool isModule = false)
        {
            if (ModelState.IsValid)
            {
                var students = await _studentService.GetStudentsByClassID(classID,startPage,endPage,quantity,userId);
                if (isModule)
                {
                    // Update progress
                    if(sessionId is not null)
                    {
                        var completedWorkAmount = students.Count();
                        if (completedWorkAmount > 0)
                        {
                            _ = UpdateAttendancePreparationProgress(completedWorkAmount, sessionId ?? 0);
                        }
                    }
                    return Ok(_mapper.Map<IEnumerable<StudentModuleResponse>>(students));
                }
                return Ok(_mapper.Map<IEnumerable<StudentResponse>>(students));
            }

            return BadRequest(new
            {
                Title = "Get Students information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("get-students-by-classId-v2")]
        public async Task<IActionResult> GetAllStudentsv2([FromQuery] int? classID, [FromQuery] int startPage, [FromQuery] int endPage, [FromQuery] int quantity, [FromQuery] int? sessionId, [FromQuery] Guid? userId, [FromQuery] bool isModule = false)
        {
            if (ModelState.IsValid)
            {
                var students = await _studentService.GetStudentsByClassIdv2(startPage, endPage, quantity, userId, classID);
                if (isModule)
                {
                    // Update progress
                    if (sessionId is not null)
                    {
                        var completedWorkAmount = students.Count();
                        if (completedWorkAmount > 0)
                        {
                            _ = UpdateAttendancePreparationProgress(completedWorkAmount, sessionId ?? 0);
                        }
                    }
                    return Ok(_mapper.Map<IEnumerable<StudentModuleResponse>>(students));
                }
                return Ok(_mapper.Map<IEnumerable<StudentResponse>>(students));
            }

            return BadRequest(new
            {
                Title = "Get Students information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            if (ModelState.IsValid)
            {
                var result = await _studentService.Delete(id);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest(new
            {
                Title = "Delete student failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost("add-students-to-class")]
        public async Task<IActionResult> AddStudentsToClass([FromBody] List<StudentClassVM> newEntities,int semesterId)
        {
            if (newEntities == null || newEntities.Count == 0)
            {
                return BadRequest("No student-class entities provided.");
            }

            var result = await _studentService.AddStudentToClass(newEntities, semesterId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(Guid id)
        {
            if(ModelState.IsValid)
            {
                var existedStudent = await _studentService.GetById(id);
                if (existedStudent is null) 
                {
                    return NotFound(new
                    {
                        Title = "Student not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<StudentResponseVM>(existedStudent)
                });
            }
            return BadRequest(new
            {
                Title = "Get student information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }


        private Task UpdateAttendancePreparationProgress(int completedWorkAmount, int sessionId)
        {
            return Task.Run(() =>
            {
                _sessionManager.UpdateSchedulePreparationProgress(sessionId, completedWorkAmount);
            }); 
        }
    }
}
