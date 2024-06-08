using AutoMapper;
using Base.IService.IService;
using Base.Service.IService;
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
    }
}
