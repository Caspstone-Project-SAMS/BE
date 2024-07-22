﻿using AutoMapper;
using Base.API.Service;
using Base.IService.IService;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Text;
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
    private readonly SessionManager _sessionManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly HangfireService _hangFireService;

    public ModuleController(IModuleService moduleService, 
        IMapper mapper, 
        IStudentService studentService,
        WebSocketConnectionManager1 websocketConnectionManager,
        IScheduleService scheduleService,
        SessionManager sessionManager,
        ICurrentUserService currentUserService,
        HangfireService hangFireService)
    {
        _moduleService = moduleService;
        _mapper = mapper;
        _studentService = studentService;
        _websocketConnectionManager = websocketConnectionManager;
        _scheduleService = scheduleService;
        _sessionManager = sessionManager;
        _currentUserService = currentUserService;
        _hangFireService = hangFireService;
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
                    Result = _mapper.Map<IEnumerable<ModuleResponseVM>>(result.Result)
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
            var userId = new Guid(_currentUserService.UserId);
            var cts = new CancellationTokenSource();

            switch (activateModule.Mode)
            {
                // Mode 1 - register fingerprint
                case 1:
                    if (activateModule.RegisterMode is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input: RegisterMode not valid" }
                        });
                    }
                    if(activateModule.SessionId is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input: Session id not found" }
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

                    var sessionResultMode1 = _sessionManager.CreateFingerRegistrationSession(activateModule.SessionId ?? 0, 
                        activateModule.RegisterMode.FingerRegisterMode, 
                        new Guid(_currentUserService.UserId), 
                        activateModule.RegisterMode.StudentID);

                    if (!sessionResultMode1)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Session is not started" }
                        });
                    }

                    var messageSendMode1 = new WebsocketMessage
                    {
                        Event = "RegisterFingerprint",
                        Data = new
                        {
                            StudentCode = existedStudent.Student?.StudentCode ?? "",
                            StudentID = existedStudent.Student?.StudentID ?? Guid.Empty,
                            Mode = activateModule.RegisterMode.FingerRegisterMode,
                            SessionID = activateModule.SessionId
                        }
                    };
                    var jsonPayloadMode1 = JsonSerializer.Serialize(messageSendMode1);

                    var resultMode1 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode1, activateModule.ModuleID);
                    if (resultMode1)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        if (await WaitForModuleMode1(cts.Token, activateModule.ModuleID, activateModule.SessionId ?? 0))
                        {
                            return Ok(new
                            {
                                Title = "Activate module successfully",
                            });
                        }
                    }

                    _sessionManager.SessionError(activateModule.SessionId ?? 0, new List<string>() { "Module is not being connected" });
                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Module is not being connected" }
                    });


                // Mode 2 - cancel session
                case 2:
                    if (activateModule.SessionId is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Cancel sesion failed",
                            Errors = new string[1] { "Invalid input: Session id not found" }
                        });
                    }
                    var sessionMode2 = _sessionManager.GetSessionById(activateModule.SessionId ?? 0);
                    if (sessionMode2 is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Cancel session failed",
                            Errors = new string[1] { "Invalid Input: Session id not found" }
                        });
                    }

                    if(sessionMode2.SessionState == 2)
                    {
                        return BadRequest(new
                        {
                            Title = "Cancel session failed",
                            Errors = new string[1] { "Session is already cancelled" }
                        });
                    }

                    var socketMode2 = _websocketConnectionManager.GetModuleSocket(sessionMode2.ModuleId);
                    if(socketMode2 is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Cancel session failed",
                            Errors = new string[1] { "Module is not being connected" }
                        });
                    }

                    var messageSendMode2 = new WebsocketMessage
                    {
                        Event = "CancelSession",
                        Data = new
                        {
                            SessionID = activateModule.SessionId
                        }
                    };
                    var jsonPayloadMode2 = JsonSerializer.Serialize(messageSendMode2);
                    var resultMode2 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode2, activateModule.ModuleID);
                    if (resultMode2)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        if (await WaitForModuleCanceling(cts.Token, activateModule.ModuleID, activateModule.SessionId ?? 0))
                        {
                            _sessionManager.CancelSession(activateModule.SessionId ?? 0, userId);
                            return Ok(new
                            {
                                Title = "Cancel session successfully"
                            });
                        }
                    }
                    return BadRequest(new
                    {
                        Title = "Cancel session failed",
                        Errors = new string[1] { "Module is not being connected" }
                    });


                // Mode 3 - prepare attendance session
                case 3:
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
                    var messageSendMode3 = new WebsocketMessage
                    {
                        Event = "PrepareAttendance",
                        Data = existedSschedule.ScheduleID.ToString()
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


                // Mode 4 - stop attendance session
                case 4:
                    if (activateModule.StopAttendance is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input" }
                        });
                    }
                    var existedSscheduleMode4 = await _scheduleService.GetById(activateModule.StopAttendance.ScheduleID);
                    if (existedSscheduleMode4 is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Schedule not found" }
                        });
                    }
                    var messageSendMode4 = new WebsocketMessage
                    {
                        Event = "StopAttendance",
                        Data = existedSscheduleMode4.ScheduleID.ToString()
                    };
                    var jsonPayloadMode4 = JsonSerializer.Serialize(messageSendMode4);
                    var resultMode4 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode4, activateModule.ModuleID);
                    if (resultMode4)
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


                // Mode 5 - prepare attendances for a day
                case 5:
                    break;


                // Mode 6 - connect module
                case 6:
                    var sessionIdMode6 = _sessionManager.CreateSession(activateModule.ModuleID, new Guid(_currentUserService.UserId));
                    var messageSendMode6 = new WebsocketMessage
                    {
                        Event = "ConnectModule",
                        Data = new
                        {
                            SessionID = sessionIdMode6,
                            User = _currentUserService.UserName
                        }
                    };
                    var jsonPayloadMode6 = JsonSerializer.Serialize(messageSendMode6);
                    var resultMode6 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode6, activateModule.ModuleID);
                    if (resultMode6)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        if (await WaitForModuleConnecting(cts.Token, activateModule.ModuleID, sessionIdMode6))
                        {
                            return Ok(new
                            {
                                Title = "Connect module successfully",
                                Result = new
                                {
                                    SessionId = sessionIdMode6
                                }
                            });
                        }
                    }
                    _sessionManager.DeleteSession(sessionIdMode6);
                    return BadRequest(new
                    {
                        Title = "Connect module failed"
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

    private async Task<bool> WaitForModuleMode1(CancellationToken cancellationToken, int moduleId, int sessionId)
    {
        var socket = _websocketConnectionManager.GetModuleSocket(moduleId);
        if (socket is null) return false;
        byte[] buffer = new byte[1024];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receiveData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (receiveData == ("Fingerprint registration " + sessionId))
                    {
                        return true;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    private async Task<bool> WaitForModuleConnecting(CancellationToken cancellationToken, int moduleId, int sessionId)
    {
        var socket = _websocketConnectionManager.GetModuleSocket(moduleId);
        if (socket is null) return false;
        byte[] buffer = new byte[1024];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receiveData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (receiveData == ("Connected " + sessionId))
                    {
                        return true;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    private async Task<bool> WaitForModuleCanceling(CancellationToken cancellationToken, int moduleId, int sessionId)
    {
        var socket = _websocketConnectionManager.GetModuleSocket(moduleId);
        if (socket is null) return false;
        byte[] buffer = new byte[1024];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receiveData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (receiveData == ("Cancelled " + sessionId))
                    {
                        return true;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateModule(ModuleVM resource,int id)
    {
        var result = await _moduleService.Update(resource,id);

        if (result.IsSuccess)
        {
            var prepareTime = result.Result!.PreparedTime;
            var autoPrepare = result.Result!.AutoPrepare;
            if (autoPrepare)
            {
                _hangFireService.ConfigureRecurringJobsAsync($"Prepare for module {id}", prepareTime, id);
            }

            _hangFireService.RemoveRecurringJobsAsync($"Prepare for module {id}");
            
            return Ok(result.Title);
        }

        return BadRequest(result.Title);
    }
}

public class ActivateModule
{
    [Required]
    public int ModuleID { get; set; }
    [Required]
    public int Mode { get; set; }
    public int? SessionId { get; set; }
    public RegisterMode? RegisterMode { get; set; }
    public StartAttendance? StartAttendance { get; set; }
    public StopAttendance? StopAttendance { get; set; }

}

public class RegisterMode
{
    public Guid StudentID { get; set; }
    public int FingerRegisterMode { get; set; }
}

public class CancelRegisterMode
{
    public int MyProperty { get; set; }
}

public class StartAttendance
{
    public int ScheduleID { get; set; }
}

public class StopAttendance
{
    public int ScheduleID { get; set; }
}
