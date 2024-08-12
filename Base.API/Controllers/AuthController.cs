﻿using AutoMapper;
using Base.API.Service;
using Base.Repository.Identity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using IMailService = Base.Service.Common.IMailService;
using Message = Base.Service.Common.Message;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJWTTokenService<IdentityUser<Guid>> _jwtTokenService;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IUserService userService, IJWTTokenService<IdentityUser<Guid>> jwtTokenService, IMapper mapper, IMailService mailService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
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
                var result = await _userService.LoginWithGoogle(resource.AccessToken);
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


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.ResetPassword(resource);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title
                    });
                }
                return BadRequest(new
                {
                    Title = result.Title,
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Reset password failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
        

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromQuery] string email)
        {
            if(ModelState.IsValid && email != string.Empty)
            {
                var result = await _userService.ForgetPassword(email);
                if (result.IsSuccess)
                {
                    if (result.ForgetPasswordUrl is not null)
                    {
                        var url = result.ForgetPasswordUrl;
                        await _mailService.SendMailAsync(new Message
                        {
                            To = result.User!.Email,
                            Subject = "Reset Password",
                            Content = "<h2>Follow the instructions to reset your password</h2>" +
                                $"<p>To reset your password <a href='{url}'>Click here</a></p>"
                        });
                    }
                    return Ok(new
                    {
                        Title = result.Title
                    });
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
                Title = "Request to reset password failed",
                Errors = new string[1] { "Invalid input" }
            });
        }


        [HttpPost("forget-password/reset-password")]
        public async Task<IActionResult> ForgetThenResetPassword([FromForm] ForgetPasswordVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.ForgetAndResetPasswordAsync(resource);
                if (result.IsSuccess)
                {
                    if(_httpContextAccessor.HttpContext is null)
                    {
                        return Ok(new
                        {
                            Title = "Reset password successfully"
                        });
                    }
                    string host = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host;
                    return Redirect($"{host}/resetpassword.html");
                }
                return BadRequest(new
                {
                    Title = result.Title,
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Reset password failed",
                Errors = new List<string> { "Invalid input" }
            });
        }
    }
}
