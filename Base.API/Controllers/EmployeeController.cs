using AutoMapper;
using Base.API.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;
        public EmployeeController(IEmployeeService employeeService, IMapper mapper)
        {
            _employeeService = employeeService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            if (ModelState.IsValid)
            {
                var employee = await _employeeService.GetById(id);
                if (employee is null) 
                {
                    return NotFound(new
                    {
                        Title = "Employee not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<EmployeeResponseVM>(employee)
                });
            }
            return BadRequest(new
            {
                Title = "Get employee information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
