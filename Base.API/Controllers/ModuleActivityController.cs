﻿using AutoMapper;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleActivityController : ControllerBase
    {
        private readonly IModuleActivityService _moduleActivityService;
        private readonly IMapper _mapper;

        public ModuleActivityController(IModuleActivityService moduleActivityService, IMapper mapper)
        {
            _mapper = mapper;
            _moduleActivityService = moduleActivityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllModuleActivity(
            [FromQuery] int startPage,
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] string? title,
            [FromQuery] string? description,
            [FromQuery] Guid? userId,
            [FromQuery] DateTime? activityDate,
            [FromQuery] bool? IsSuccess,
            [FromQuery] int? moduleId,
            [FromQuery] int? scheduleId,
            [FromQuery] bool noDuplicate = true)
        {
            if (ModelState.IsValid)
            {
                var result = await _moduleActivityService.GetAll(startPage, endPage, quantity, title, description, userId, activityDate, IsSuccess, moduleId, scheduleId);
                if (result.IsSuccess)
                {
                    var activities = result.Result;
                    if (noDuplicate)
                    {
                        var newActivityList = new List<ModuleActivity>();
                        var activityGroups = activities?.GroupBy(a => a.ModuleID);
                        if(activityGroups is not null && activityGroups.Any())
                        {
                            foreach (var group in activityGroups)
                            {
                                // Get the last activity in duplicate activities
                                var lastTime = group.Max(a => a.StartTime);
                                newActivityList.Add(group.First(a => a.StartTime == lastTime));
                            }
                            activities = newActivityList ?? Enumerable.Empty<ModuleActivity>();
                        }
                    }
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<IEnumerable<ModuleActivityResponseVM>>(activities)
                    });
                }
                return BadRequest(new
                {
                    Title = "Get module activity falied",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get module activities failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetModuleActivityById(int id)
        {
            if(ModelState.IsValid && id > 0)
            {
                var existedModuleActivity = await _moduleActivityService.GetById(id);
                if(existedModuleActivity is null)
                {
                    return NotFound(new
                    {
                        Title = "Module activity not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<ModuleActivityResponseVM>(existedModuleActivity)
                });
            }
            return BadRequest(new
            {
                Title = "Get module activity failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
