using AutoMapper;
using Base.Repository.Identity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;
        public RoleController(IRoleService roleService, IMapper mapper)
        {
            _roleService = roleService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetRoleById))]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleService.GetById(id);
                if(role is null)
                {
                    return NotFound(new
                    {
                        Title = "Role not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<RoleResponseVM>(role)
                });
            }

            return BadRequest(new
            {
                Title = "Get role information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles([FromQuery] int startPage, [FromQuery] int endPage, [FromQuery] int? quantity, [FromQuery] string? roleName)
        {
            if (ModelState.IsValid)
            {
                var roles = await _roleService.Get(startPage, endPage, quantity, roleName);
                return Ok(new
                {
                    Result = _mapper.Map<IEnumerable<RoleResponseVM>>(roles)
                });
            }

            return BadRequest(new
            {
                Title = "Get roles information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.Create(_mapper.Map<Role>(resource));
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetRoleById), new
                    {
                        id = result.Result!.Id,
                    }, _mapper.Map<RoleResponseVM>(result.Result!));
                }

                return BadRequest(result);
            }

            return BadRequest(new
            {
                Title = "Create role failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleVM resource, Guid id)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.Update(_mapper.Map<Role>(resource), id);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<RoleResponseVM>(result.Result)
                    });
                }

                return BadRequest(result);
            }

            return BadRequest(new
            {
                Title = "Update role failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.Delete(id);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest(new
            {
                Title = "Delete role failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
