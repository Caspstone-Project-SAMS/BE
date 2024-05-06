using AutoMapper;
using Base.API.Service;
using Base.Repository.Identity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJWTTokenService<IdentityUser<Guid>> _jwtTokenService;
        private readonly IMapper _mapper;

        public AuthController(IUserService userService, IJWTTokenService<IdentityUser<Guid>> jwtTokenService, IMapper mapper)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUser(resource);
                if (result.IsSuccess)
                {
                    var tokenString = _jwtTokenService.CreateToken(result.LoginUser!, result.RoleNames);
                    if (tokenString is not null)
                    {
                        return Ok(new AuthenticateResponseVM
                        {
                            Token = tokenString,
                            Result = _mapper.Map<UserInformationResponseVM>(result.LoginUser!)
                        });
                    }
                    else
                    {
                        return StatusCode(500, new
                        {
                            Title = "Login failed",
                            Errors = new List<string>() { "Can not create token" }
                        });
                    }
                }
                else
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = result.Title,
                        Errors = result.Errors
                    });
                }
            }

            return BadRequest(new
            {
                Title = "Login failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost("login/google")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> LoginGoogle([FromBody] LoginGoogleVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginWithGoogle(resource.IdToken);
                if (result.IsSuccess)
                {
                    var tokenString = _jwtTokenService.CreateToken(result.LoginUser!, result.RoleNames);
                    if (tokenString is not null)
                    {
                        return Ok(new AuthenticateResponseVM
                        {
                            Token = tokenString,
                            Result = _mapper.Map<UserInformationResponseVM>(result.LoginUser!)
                        });
                    }
                    else
                    {
                        return StatusCode(500, new
                        {
                            Title = "Login failed",
                            Errors = new List<string>() { "Can not create token" }
                        });
                    }
                }
                else
                {
                    return BadRequest(new
                    {
                        Title = result.Title,
                        Errors = result.Errors
                    });
                }
            }

            return BadRequest(new
            {
                Title = "Login failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
