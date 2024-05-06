using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserInformationResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateNewUser([FromForm] UserVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.CreateNewUser(resource);
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetUserById), new
                    {
                        id = result.Result!.Id
                    }, _mapper.Map<UserInformationResponseVM>(result.Result));
                }
                return BadRequest(_mapper.Map<ServiceResponseVM>(result));
            }
            else
            {
                return BadRequest(new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Invalid input"
                });
            }
        }

        [HttpGet("{id}", Name = nameof(GetUserById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInformationResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            if (ModelState.IsValid)
            {
                var userResult = await _userService.GetUserById(id);
                if (userResult is null)
                {
                    return NotFound("Not Found");
                }
                return Ok(new ServiceResponseVM<UserInformationResponseVM>
                {
                    Result = _mapper.Map<UserInformationResponseVM>(userResult)
                });
            }
            else
            {
                return BadRequest(new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Invalid input"
                });
            }
        }
    }
}
