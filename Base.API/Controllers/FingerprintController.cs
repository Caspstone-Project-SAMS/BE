using AutoMapper;
using Base.API.Service;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FingerprintController : ControllerBase
{
    private readonly IFingerprintService _fingerprintService;
    private readonly IMapper _mapper;
    private readonly WebSocketConnectionManager1 _webSocketConnectionManager;
    private readonly SessionManager _sessionManager;
    public FingerprintController(IFingerprintService fingerprintService, 
        IMapper mapper, 
        WebSocketConnectionManager1 webSocketConnectionManager,
        SessionManager sessionManager)
    {
        _fingerprintService = fingerprintService;
        _webSocketConnectionManager = webSocketConnectionManager;
        _mapper = mapper;
        _sessionManager = sessionManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewFingerprint([FromBody] FingerprintVM resource)
    {
        if (ModelState.IsValid)
        {
            // Need to check session, add finger to session
            var sessionCheck = _sessionManager.RegisterFinger(resource.SessionID, resource.FingerprintTemplate, resource.FingerNumber, resource.StudentID);
            if (!sessionCheck)
            {
                return BadRequest(new
                {
                    Title = "Invalid session",
                    Errors = new string[1] { "Invalid session" }
                });
            }

            // Notify to admin
            var messageSend = new WebsocketMessage()
            {
                Event = "RegisterFingerSuccessfully",
                Data = new
                {
                    SessionID = resource.SessionID,
                    StudentID = resource.StudentID,
                    Finger = resource.FingerNumber
                }
            };
            var messageSendString = JsonSerializer.Serialize(messageSend);
            // Send to admin who have the session
            var session = _sessionManager.GetSessionById(resource.SessionID);
            await _webSocketConnectionManager.SendMessageToClient(messageSendString, session?.UserID ?? Guid.Empty);

            return Ok(new
            {
                Title = "Register successfully"
            });
        }
        return BadRequest(new
        {
            Title = "Register fingerprint failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateFingerprint([FromBody] FingerprintUpdateVM resource)
    {
        if (ModelState.IsValid)
        {
            // Need to check session, add finger to session
            var sessionCheck = _sessionManager.UpdateFinger(resource.SessionID, resource.FingerprintTemplate, resource.FingerTemplateId, resource.StudentID);
            if (!sessionCheck)
            {
                return BadRequest(new
                {
                    Title = "Invalid session",
                    Errors = new string[1] { "Invalid session" }
                });
            }

            // Notify to admin
            var messageSend = new WebsocketMessage()
            {
                Event = "UpdateFingerSuccessfully",
                Data = new
                {
                    SessionID = resource.SessionID,
                    StudentID = resource.StudentID,
                    FingerID = resource.FingerTemplateId
                }
            };
            var messageSendString = JsonSerializer.Serialize(messageSend);
            // Send to admin who have the session
            var session = _sessionManager.GetSessionById(resource.SessionID);
            await _webSocketConnectionManager.SendMessageToClient(messageSendString, session?.UserID ?? Guid.Empty);

            return Ok(new
            {
                Title = "Update successfully"
            });
        }
        return BadRequest(new
        {
            Title = "Update fingerprint failed",
            Errors = new string[1] { "Invalid input" }
        });
    }
}
