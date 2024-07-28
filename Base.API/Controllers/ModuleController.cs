using AutoMapper;
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
using Base.API.Common;

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
    private readonly WebsocketEventManager _websocketEventManager;
    private readonly WebsocketEventState websocketEventState = new WebsocketEventState();

    public ModuleController(IModuleService moduleService, 
        IMapper mapper, 
        IStudentService studentService,
        WebSocketConnectionManager1 websocketConnectionManager,
        IScheduleService scheduleService,
        SessionManager sessionManager,
        ICurrentUserService currentUserService,
        HangfireService hangFireService,
        WebsocketEventManager websocketEventManager)
    {
        _moduleService = moduleService;
        _mapper = mapper;
        _studentService = studentService;
        _websocketConnectionManager = websocketConnectionManager;
        _scheduleService = scheduleService;
        _sessionManager = sessionManager;
        _currentUserService = currentUserService;
        _hangFireService = hangFireService;
        _websocketEventManager = websocketEventManager;
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
            var websocketEventHandler = _websocketEventManager.GetHandlerByModuleID(activateModule.ModuleID);
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

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.RegisterFingerprintEvent += OnModuleMode1EventHandler;
                    }
                    websocketEventState.SessionId = activateModule.SessionId ?? 0;

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
                        if (WaitForModuleMode1(cts.Token))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.RegisterFingerprintEvent -= OnModuleMode1EventHandler;
                            }

                            return Ok(new
                            {
                                Title = "Activate module successfully",
                            });
                        }
                    }

                    // If a fingerprint registration session is cancelled, dont delete it
                    // We dont record the activity of fingerprint registration
                    _sessionManager.SessionError(activateModule.SessionId ?? 0, new List<string>() { "Module is not being connected" });

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.RegisterFingerprintEvent -= OnModuleMode1EventHandler;
                    }

                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Connection times out" }
                    });


                // Mode 2 - cancel session
                case 2:
                    if (activateModule.SessionId is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Cancel sesion failed",
                            Errors = new string[1] { "Session not found" }
                        });
                    }
                    var sessionMode2 = _sessionManager.GetSessionById(activateModule.SessionId ?? 0);
                    if (sessionMode2 is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Cancel session failed",
                            Errors = new string[1] { "Session not found" }
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

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.CancelSessionEvent += OnModuleMode2EventHandler;
                    }
                    websocketEventState.SessionId = activateModule.SessionId ?? 0;

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
                        if (WaitForModuleCanceling(cts.Token))
                        {
                            // Cancel will do a specific work with each category of session
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
                        Errors = new string[1] { "Connection timed out" }
                    });


                // Mode 3 - prepare attendance session
                case 3:
                    if (activateModule.PrepareAttendance is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input" }
                        });
                    }
                    var existedSschedule = await _scheduleService.GetById(activateModule.PrepareAttendance.ScheduleID);
                    if (existedSschedule is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Schedule not found" }
                        });
                    }

                    var totalStudents = await _studentService.GetStudentsByClassID(existedSschedule.ClassID, 1, 100, 50);
                    int totalWorkAmount = 0;
                    if(totalStudents is not null)
                    {
                        totalWorkAmount = totalStudents.Count();

                    }

                    var sessionResultMode3 = _sessionManager.CreatePrepareAScheduleSession(activateModule.SessionId ?? 0,
                        activateModule.PrepareAttendance.ScheduleID,
                        totalWorkAmount
                        );

                    if (!sessionResultMode3)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Session is not started" }
                        });
                    }

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.PrepareAttendanceSession += OnModuleMode3EventHandler;
                    }
                    websocketEventState.SessionId = activateModule.SessionId ?? 0;

                    var messageSendMode3 = new WebsocketMessage
                    {
                        Event = "PrepareAttendance",
                        Data = new
                        {
                            ScheduleID = existedSschedule.ScheduleID,
                            SessionID = activateModule.SessionId
                        }
                    };
                    var jsonPayloadMode3 = JsonSerializer.Serialize(messageSendMode3);
                    var resultMode3 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode3, activateModule.ModuleID);
                    if (resultMode3)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        if (WaitForModuleMode3(cts.Token))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.PrepareAttendanceSession -= OnModuleMode3EventHandler;
                            }

                            return Ok(new
                            {
                                Title = "Activate module successfully",
                            });
                        }
                    }


                    // Session got error -> lets complete it, record its activity and delete it.
                    _sessionManager.SessionError(activateModule.SessionId ?? 0, new List<string>() { "Module is not being connected" });
                    _ = _sessionManager.CompleteSession(activateModule.SessionId ?? 0, false);

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.PrepareAttendanceSession -= OnModuleMode3EventHandler;
                    }

                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Module is not being connected" }
                    });


                // Mode 4 - stop attendance session
                // call this just for stop the attendance of module - does not effect to the session management of the system
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

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.StopAttendance += OnModuleMode4EventHandler;
                    }
                    websocketEventState.ScheduleId = activateModule.StopAttendance.ScheduleID;

                    var messageSendMode4 = new WebsocketMessage
                    {
                        Event = "StopAttendance",
                        Data = new
                        {
                            ScheduleID = existedSscheduleMode4.ScheduleID
                        }
                    };
                    var jsonPayloadMode4 = JsonSerializer.Serialize(messageSendMode4);
                    var resultMode4 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode4, activateModule.ModuleID);
                    if (resultMode4)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        if (WaitForModuleMode4(cts.Token))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.StopAttendance -= OnModuleMode4EventHandler;
                            }

                            return Ok(new
                            {
                                Title = "Stop attendance successfully",
                            });
                        }
                    }

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.StopAttendance -= OnModuleMode4EventHandler;
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
                // If connect module failed, there is no session created and no activity recorded
                case 6:
                    var sessionIdMode6 = _sessionManager.CreateSession(activateModule.ModuleID, new Guid(_currentUserService.UserId), 1);

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.ConnectModuleEvent += OnModuleConnectingEventHandler;
                    }
                    websocketEventState.SessionId = sessionIdMode6;

                    var messageSendMode6 = new WebsocketMessage
                    {
                        Event = "ConnectModule",
                        Data = new
                        {
                            SessionID = sessionIdMode6,
                            User = _currentUserService.UserName,
                            DurationInMin = 1
                        }
                    };
                    var jsonPayloadMode6 = JsonSerializer.Serialize(messageSendMode6);
                    var resultMode6 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode6, activateModule.ModuleID);
                    if (resultMode6)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        if (WaitForModuleConnecting(cts.Token))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.ConnectModuleEvent -= OnModuleConnectingEventHandler;
                            }

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

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.ConnectModuleEvent -= OnModuleConnectingEventHandler;
                    }

                    return BadRequest(new
                    {
                        Title = "Connect module failed",
                        Errors = new string[1] { "Connection times out" }
                    });


                // Mode 7 - setup module
                case 7:
                    break;


                // Mode 8 - update fingerprint
                case 8:
                    if (activateModule.RegisterMode is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input: RegisterMode not valid" }
                        });
                    }
                    if (activateModule.SessionId is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Invalid input: Session id not found" }
                        });
                    }

                    var existedStudentMode8 = await _studentService.GetById(activateModule.RegisterMode.StudentID);
                    if (existedStudentMode8 is null)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Student not found" }
                        });
                    }

                    var sessionResultMode8 = _sessionManager.CreateFingerUpdateSession(activateModule.SessionId ?? 0,
                        activateModule.RegisterMode.FingerRegisterMode,
                        new Guid(_currentUserService.UserId),
                        activateModule.RegisterMode.StudentID);

                    if (!sessionResultMode8)
                    {
                        return BadRequest(new
                        {
                            Title = "Activate module failed",
                            Errors = new string[1] { "Session is not started" }
                        });
                    }

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.RegisterFingerprintEvent += OnModuleMode1EventHandler;
                    }
                    websocketEventState.SessionId = activateModule.SessionId ?? 0;

                    var messageSendMode8 = new WebsocketMessage
                    {
                        Event = "RegisterFingerprint",
                        Data = new
                        {
                            StudentCode = existedStudentMode8.Student?.StudentCode ?? "",
                            StudentID = existedStudentMode8.Student?.StudentID ?? Guid.Empty,
                            Mode = activateModule.RegisterMode.FingerRegisterMode,
                            SessionID = activateModule.SessionId
                        }
                    };
                    var jsonPayloadMode8 = JsonSerializer.Serialize(messageSendMode8);
                    var resultMode8 = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode8, activateModule.ModuleID);
                    if (resultMode8)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(10));
                        if (WaitForModuleMode1(cts.Token))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.RegisterFingerprintEvent -= OnModuleMode1EventHandler;
                            }

                            return Ok(new
                            {
                                Title = "Activate module successfully",
                            });
                        }
                    }

                    // If a fingerprint registration session is cancelled, dont delete it
                    // We dont record the activity of fingerprint registration
                    _sessionManager.SessionError(activateModule.SessionId ?? 0, new List<string>() { "Module is not being connected" });

                    if (websocketEventHandler is not null)
                    {
                        websocketEventHandler.RegisterFingerprintEvent -= OnModuleMode1EventHandler;
                    }

                    return BadRequest(new
                    {
                        Title = "Activate module failed",
                        Errors = new string[1] { "Connection times out" }
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
            else
            {
                _hangFireService.RemoveRecurringJobsAsync($"Prepare for module {id}");
            }
            
            return Ok(result.Title);
        }

        return BadRequest(result.Title);
    }


    private bool WaitForModuleMode1(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (websocketEventState.ModuleMode1)
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    private bool WaitForModuleConnecting(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (websocketEventState.ModuleConnected)
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    private bool WaitForModuleCanceling(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (websocketEventState.ModuleMode2)
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    private bool WaitForModuleMode3(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (websocketEventState.ModuleMode3)
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    private bool WaitForModuleMode4(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (websocketEventState.ModuleMode4)
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }



    private void OnModuleConnectingEventHandler(object? sender, WebsocketEventArgs e)
    {

        if(e.Event == ("Connected " + websocketEventState.SessionId))
        {
            websocketEventState.ModuleConnected = true;
        }
    }

    private void OnModuleMode1EventHandler(object? sender, WebsocketEventArgs e)
    {
        if(e.Event == ("Register fingerprint " + websocketEventState.SessionId))
        {
            websocketEventState.ModuleMode1 = true;
        }
    }

    private void OnModuleMode2EventHandler(object? sender, WebsocketEventArgs e)
    {
        if(e.Event == ("Cancel session " + websocketEventState.SessionId))
        {
            websocketEventState.ModuleMode2 = true;
        }
    }

    private void OnModuleMode3EventHandler(object? sender, WebsocketEventArgs e)
    {
        if(e.Event == ("Prepare attendance " + websocketEventState.SessionId))
        {
            websocketEventState.ModuleMode3 = true;
        }
    }

    private void OnModuleMode4EventHandler(object? sender, WebsocketEventArgs e)
    {
        if (e.Event == ("Stop attendance " + websocketEventState.ScheduleId))
        {
            websocketEventState.ModuleMode4 = true;
        }
    }
}


public class WebsocketEventState
{
    public int SessionId { get; set; }
    public int ScheduleId { get; set; }
    public bool ModuleConnected { get; set; } = false;
    public bool ModuleMode1 { get; set; } = false;
    public bool ModuleMode2 { get; set; } = false;
    public bool ModuleMode3 { get; set; } = false;
    public bool ModuleMode4 { get; set; } = false;
}

public class ActivateModule
{
    [Required]
    public int ModuleID { get; set; }
    [Required]
    public int Mode { get; set; }
    public int? SessionId { get; set; }
    public RegisterMode? RegisterMode { get; set; }
    public PrepareAttendance? PrepareAttendance { get; set; }
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

public class PrepareAttendance
{
    public int ScheduleID { get; set; }
}

public class StopAttendance
{
    public int ScheduleID { get; set; }
}
