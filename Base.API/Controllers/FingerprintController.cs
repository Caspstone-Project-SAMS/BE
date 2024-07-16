using AutoMapper;
using Base.API.Service;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FingerprintController : ControllerBase
    {
        private readonly IFingerprintService _fingerprintService;
        private readonly IMapper _mapper;
        private readonly WebSocketConnectionManager1 _webSocketConnectionManager;
        public FingerprintController(IFingerprintService fingerprintService, IMapper mapper, WebSocketConnectionManager1 webSocketConnectionManager)
        {
            _fingerprintService = fingerprintService;
            _webSocketConnectionManager = webSocketConnectionManager;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewFingerprint([FromBody] FingerprintVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _fingerprintService.CreateNewFinger(resource.StudentID, resource.FingerprintTemplate);
                if (result.IsSuccess)
                {
                    // Notify to admin
                    var messageSend = new MessageSend()
                    {
                        Event = "RegisterFingerSuccessfully",
                        Data = new
                        {
                            StudentID = resource.StudentID,
                            Finger = resource.SessionID
                        }
                    };
                    var messageSendString = JsonSerializer.Serialize(messageSend);
                    _webSocketConnectionManager.SendMessageToAllClient(messageSendString);
                    return Ok(result);
                }
                return BadRequest(result);
            }
            return BadRequest(new
            {
                Title = "Register fingerprint failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
