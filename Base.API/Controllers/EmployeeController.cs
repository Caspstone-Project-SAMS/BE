using AutoMapper;
using Base.API.Common;
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

        [HttpGet]
        public async Task<IActionResult> GetAllEmployee(
            [FromQuery] int startPage, 
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] string? email,
            [FromQuery] string? phone,
            [FromQuery] string? department,
            [FromQuery] int? roleId)
        {
            if (ModelState.IsValid)
            {
                var result = await _employeeService.GetAll(startPage, endPage, quantity, email, phone, department, roleId);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<IEnumerable<EmployeeResponseVM>>(result.Result)
                    });
                }
                return BadRequest(new
                {
                    Title = "Get employees falied",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get employees failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewEmployee(List<EmployeeVM> resources)
        {
            var result = await _employeeService.CreateEmployee(resources);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
