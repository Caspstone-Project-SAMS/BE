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
    }
}
