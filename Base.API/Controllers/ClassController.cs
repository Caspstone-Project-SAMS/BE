using AutoMapper;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
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

        [HttpGet]
        public async Task<IActionResult> GetClassDetail([FromQuery] int scheduleID)
        {
            var classDetail = await _classService.GetClassDetail(scheduleID);
            if (classDetail == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<ClassResponse>(classDetail));
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass(ClassVM resource)
        {
            var response = await _classService.Create(resource);
            if (response.IsSuccess)
            {
                return Ok(response.Title);
            }
            return BadRequest(response);
        }

        [HttpGet("get-all-class")]
        public async Task<IActionResult> GetAllClasses([FromQuery] int startPage, [FromQuery] int endPage, [FromQuery] Guid? lecturerId, [FromQuery] int quantity, [FromQuery] int? semesterId, [FromQuery] string? classCode)
        {
            var classes = await _classService.Get(startPage, endPage, lecturerId, quantity, semesterId, classCode);
            if (classes == null)
            {
                return NotFound();
            }

            return Ok(classes);
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

