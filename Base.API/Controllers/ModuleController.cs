using AutoMapper;
using Base.API.Common;
using Base.IService.IService;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModuleController : ControllerBase
{
    private readonly IModuleService _moduleService;
    private readonly IMapper _mapper;
    private readonly IStudentService _studentService;
    private readonly IScheduleService _scheduleService;
    private readonly WebSocketConnectionManager1 _websocketConnectionManager;
    public ModuleController(IModuleService moduleService, 
        IMapper mapper, 
        IStudentService studentService,
        WebSocketConnectionManager1 websocketConnectionManager,
        IScheduleService scheduleService)
    {
        _moduleService = moduleService;
        _mapper = mapper;
        _studentService = studentService;
        _websocketConnectionManager = websocketConnectionManager;
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetModules(int startPage, int endPage, int? quantity, int? mode, int? status, string? key, Guid? employeeId)
    {
        if (ModelState.IsValid)
        {
            var result = await _moduleService.Get(startPage, endPage, quantity, mode, status, key, employeeId);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title,
                    Result = _mapper.Map<ModuleResponseVM>(result.Result)
                });
            }
            return BadRequest(new
            {
                Title = "Get modules falied",
                Errors = result.Errors
            });
        }
        return BadRequest(new
        {
            Title = "Get modules failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetModuleById(int id)
    {
        if (ModelState.IsValid)
        {
            var module = await _moduleService.GetById(id);
            if(module is null)
            {
                return NotFound(new
                {
                    Title = "Module not found"
                });
            }
            return Ok(new
            {
                Result = _mapper.Map<ModuleResponseVM>(module)
            });
        }
        return BadRequest(new
        {
            Title = "Get module information failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [Authorize(Policy = "Admin Lecturer")]
    [HttpPost("Activate")]
    public async Task<IActionResult> ActivateModule([FromBody] ActivateModule activateModule)
    {
        if(ModelState.IsValid && activateModule.ModuleID > 0 && activateModule.Mode > 0)
        {
            switch (activateModule.Mode)
            {
                case 1:
                    // Mode 1 - register fingerprint
                    if (activateModule.RegisterMode is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input" }
                        });
                    }
                    var existedStudent = await _studentService.GetById(activateModule.RegisterMode.StudentID);
                    if(existedStudent is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Student not found" }
                        });
                    }
                    var messageSendMode1 = new MessageSend
                    {
                        Event = "RegisterFingerprint",
                        Data = existedStudent.Student?.StudentCode ?? ""
                    };
                    var jsonPayloadMode1 = JsonSerializer.Serialize(messageSendMode1);
                    var resultMode1 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode1, activateModule.ModuleID);
                    if (resultMode1)
                    {
                        return Ok(new
                        {
                            Title = "Activate module successfully"
                        });
                    }
                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Module is not being connected" }
                    });

                case 2:
                    // Mode 2 - start attendance session
                    if (activateModule.StartAttendance is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input" }
                        });
                    }
                    var existedSschedule = await _scheduleService.GetById(activateModule.StartAttendance.ScheduleID);
                    if (existedSschedule is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Schedule not found" }
                        });
                    }
                    var messageSendMode2 = new MessageSend
                    {
                        Event = "StartAttendance",
                        Data = existedSschedule.ScheduleID.ToString()
                    };
                    var jsonPayloadMode2 = JsonSerializer.Serialize(messageSendMode2);
                    var resultMode2 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode2, activateModule.ModuleID);
                    if (resultMode2)
                    {
                        return Ok(new
                        {
                            Title = "Activate module successfully"
                        });
                    }
                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Module is not being connected" }
                    });

                case 3:
                    // Mode 3 - stop attendance session
                    if (activateModule.StopAttendance is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input" }
                        });
                    }
                    var existedSscheduleMode3 = await _scheduleService.GetById(activateModule.StopAttendance.ScheduleID);
                    if (existedSscheduleMode3 is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Schedule not found" }
                        });
                    }
                    var messageSendMode3 = new MessageSend
                    {
                        Event = "StopAttendance",
                        Data = existedSscheduleMode3.ScheduleID.ToString()
                    };
                    var jsonPayloadMode3 = JsonSerializer.Serialize(messageSendMode3);
                    var resultMode3 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode3, activateModule.ModuleID);
                    if (resultMode3)
                    {
                        return Ok(new
                        {
                            Title = "Activate module successfully"
                        });
                    }
                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Module is not being connected" }
                    });

                default:
                    // Undefined mode
                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Mode " + activateModule.Mode + " is not defined" }
                    });
            }
        }
        return BadRequest(new
        {
            Title = "Activate module failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    
}

public class ActivateModule
{
    [Required]
    public int ModuleID { get; set; }
    [Required]
    public int Mode { get; set; }
    public RegisterMode? RegisterMode { get; set; }
    public StartAttendance? StartAttendance { get; set; }
    public StopAttendance? StopAttendance { get; set; }

}

public class RegisterMode
{
    public Guid StudentID { get; set; }
    public int FingerRegisterMode { get; set; }
}

public class StartAttendance
{
    public int ScheduleID { get; set; }
}

public class StopAttendance
{
    public int ScheduleID { get; set; }
}
