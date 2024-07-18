using AutoMapper;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly IMapper _mapper;
        public ClassController(IClassService classService, IMapper mapper)
        {
            _classService = classService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass(ClassVM resource)
        {
            var result = await _classService.Create(resource);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title,
                    Result = _mapper.Map<ClassResponseVM>(result.Result)
                });
            }
            return BadRequest(new
            {
                Title = result.Title,
                Errors = result.Errors
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses(
            [FromQuery] int startPage, 
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] int? semesterId, 
            [FromQuery] string? classCode,
            [FromQuery] int? classStatus,
            [FromQuery] int? roomID,
            [FromQuery] int? subjectID,
            [FromQuery] Guid? lecturerId,
            [FromQuery] Guid? studentId,
            [FromQuery] int? scheduleId)
        {
            if (ModelState.IsValid)
            {
                var result = await _classService.GetAllClasses(startPage, endPage, quantity, semesterId, classCode, classStatus, roomID, subjectID, lecturerId, studentId, scheduleId);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<IEnumerable<ClassResponseVM>>(result.Result)
                    });
                }
                return BadRequest(new
                {
                    Title = "Get classes falied",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get classes failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClasById(int id)
        {
            if (ModelState.IsValid && id > 0)
            {
                var existedClass = await _classService.GetById(id);
                if (existedClass is null)
                {
                    return NotFound(new
                    {
                        Title = "Class not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<ClassResponseVM>(existedClass)
                });
            }
            return BadRequest(new
            {
                Title = "Get class information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}

