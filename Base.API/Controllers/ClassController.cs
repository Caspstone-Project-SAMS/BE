using AutoMapper;
using Base.Service.IService;
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
        public async Task<IActionResult> GetClassDetail([FromQuery]int scheduleID)
        {
            var classDetail = await _classService.GetClassDetail(scheduleID);
            if (classDetail == null)
            {
                return NotFound();
            }
            return Ok(classDetail);
        }
    }
}
