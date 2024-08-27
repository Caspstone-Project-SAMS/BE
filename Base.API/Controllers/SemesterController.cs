using AutoMapper;
using Base.IService.IService;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Service;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly ISemesterService _semesterService;
        private readonly IMapper _mapper;
        public SemesterController(ISemesterService semesterService, IMapper mapper)
        {
            _semesterService = semesterService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetSemester()
        {
            var semesters = await _semesterService.GetSemester();
            if(semesters == null)
            { 
                return NotFound("Do not have any Semester");
            }
            var result = _mapper.Map<IEnumerable<SemesterResponse>>(semesters);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewSemester(SemesterVM resource)
        {
            var result = await _semesterService.Create(resource);
            if (result.IsSuccess)
            {
                return Ok("Create Semester Successfully");
            }

            return BadRequest(result);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateSemester(SemesterVM resource,int id)
        {
            var result = await _semesterService.Update(resource,id);
            if (result.IsSuccess)
            {
                return Ok("Update Semester Successfully");
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _semesterService.Delete(id);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        result.Title
                    });
                }

                return BadRequest(new
                {
                    result.Title,
                    result.Errors
                });
            }

            return BadRequest(new
            {
                Title = "Delete semester failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSemesterById(int id)
        {
            if(ModelState.IsValid && id > 0)
            {
                var existedSemester = await _semesterService.GetById(id);
                if(existedSemester is null)
                {
                    return NotFound(new
                    {
                        Title = "Semester not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<SemesterResponseVM>(existedSemester)
                });
            }
            return BadRequest(new
            {
                Title = "Get semester information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSemesters([FromQuery] int startPage,
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] string? semesterCode,
            [FromQuery] int? semesterStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            if (ModelState.IsValid)
            {
                var result = await _semesterService.GetAll(startPage, endPage, quantity, semesterCode, semesterStatus, startDate, endDate);
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
                    Title = "Get semesters falied",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get semesters failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
