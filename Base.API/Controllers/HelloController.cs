using Base.API.Service;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Base.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private readonly WebSocketConnectionManager _webSocketConnectionManager;
    private readonly WebSocketConnectionManager1 _webSocketConnectionManager1;
    private readonly SessionManager _sessionManager;

    public HelloController(WebSocketConnectionManager webSocketConnectionManager, 
        WebSocketConnectionManager1 webSocketConnectionManager1,
        SessionManager sessionManager)
    {
        _webSocketConnectionManager = webSocketConnectionManager;
        _webSocketConnectionManager1 = webSocketConnectionManager1;
        _sessionManager = sessionManager;
    }

    private static IList<FingerprintTemplate> fingerprintTemplates = new List<FingerprintTemplate>();
    [HttpGet]
    public IActionResult Hello()
    {
        return Ok("Hello");
    }


    [HttpPost("fingerprint")]
    public IActionResult AddNew([FromBody] FingerprintTemplateTest fingerprintTemplate)
    {
        int largestId = 0;
        if (fingerprintTemplates.Count >= 1)
        {
            largestId = fingerprintTemplates.Select(f => f.Id).Max();
        }
        fingerprintTemplates.Add(new FingerprintTemplate
        {
            Id = largestId + 1,
            Fingerprint = fingerprintTemplate.fingerprintTemplate,
            IsAuthenticated = false,
            Content = fingerprintTemplate.Content
        });
        return Ok("Ok");
    }

    [HttpGet("fingerprint")]
    public IActionResult GetAllFingerprintTemplates()
    {
        return Ok(fingerprintTemplates.Select(f => new 
        { 
            Id = f.Id,
            Finger = f.Fingerprint 
        }));
    }

    [HttpGet("get-all-information")]
    public IActionResult GetALlInformation()
    {
        return Ok(fingerprintTemplates);
    }

    [HttpDelete("fingerprint")]
    public IActionResult DeleteAll()
    {
        fingerprintTemplates = new List<FingerprintTemplate>();
        return Ok("Delete all");
    }

    [HttpPut("attendance/{id}")]
    public IActionResult Attendance(int id, [FromQuery] DateTime? dateTime)
    {
        var fingerprint = fingerprintTemplates.Where(f => f.Id == id).FirstOrDefault();
        if(fingerprint is null)
        {
            return NotFound("Fingerprint Id not found");
        }
        fingerprint.IsAuthenticated = true;
        if(dateTime is null)
        {
            fingerprint.ScanningTime = DateTime.UtcNow;
        }
        else
        {
            fingerprint.ScanningTime = dateTime;
        }
        return Ok("Attendance");
    }

    [HttpPost("activate-module")]
    public IActionResult ActivateRegisterFingerprint([FromQuery] string content, [FromQuery] string moduleId)
    {
        var messageSend = new WebsocketMessage
        {
            Event = "RegisterFingerprint",
            Data = content
        };
        var jsonPayload = JsonSerializer.Serialize(messageSend);
        _webSocketConnectionManager.SendMessageToModule(jsonPayload, moduleId);
        return Ok();
    }

    [HttpPost("Close-all-web-socket")]
    public async Task<IActionResult> CloseAllWebSocket()
    {
        await _webSocketConnectionManager.CloseAllSocket();
        return Ok();
    }

    [HttpGet("get-all-web-socket")]
    public IActionResult GetAllSocket()
    {
        var sockets = _webSocketConnectionManager.GetAllWebSockets().Where(s => s != null);
        return Ok(sockets);
    }

    [HttpGet("get-all-web-socket-class")]
    public IActionResult GetAllSocketClass()
    {
        var sockets = _webSocketConnectionManager.GetAllWebSocketsClass();
        return Ok(sockets);
    }

    [HttpGet("alooeoweiofrjwof")]
    public IActionResult GetGet()
    {
        return Ok();
    }







    [HttpGet("test-v2/all-module-websocket")]
    public IActionResult GetV2ModuleWebsocket()
    {
        return Ok(_webSocketConnectionManager1.GetAllModuleSocket());
    }

    [HttpGet("test-v2/all-client-websocket")]
    public IActionResult GetV2ClientWebsocket()
    {
        return Ok(_webSocketConnectionManager1.GetAllClientSocket());
    }

    [HttpGet("Session")]
    public IActionResult GetSessionString()
    {
        return Ok(_sessionManager.GetAllString());
    }

    [HttpGet("Session/all")]
    public IActionResult GetAllSessions()
    {
        return Ok(_sessionManager.GetAllSessions());
    }

    [HttpDelete("Session")]
    public IActionResult DeleteAllString()
    {
        _sessionManager.DeleteAllString();
        return Ok();
    }


    public class FingerprintTemplateTest
    {
        public string fingerprintTemplate { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class FingerprintTemplate
    {
        public int Id { get; set; }
        public string Fingerprint { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; } = false;
        public DateTime? ScanningTime { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}