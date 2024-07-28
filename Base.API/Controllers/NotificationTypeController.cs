using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationTypeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly INotificationTypeService _notificationTypeService;

        public NotificationTypeController(IMapper mapper, INotificationTypeService notificationTypeService)
        {
            _mapper = mapper;
            _notificationTypeService = notificationTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotificationTypes(
            [FromQuery] int startPage,
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] string? typeName,
            [FromQuery] string? typeDescription)
        {
            if (ModelState.IsValid)
            {
                var result = await _notificationTypeService.GetAll(startPage, endPage, quantity, typeName, typeDescription);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<IEnumerable<NotificationTypeResponseVM>>(result.Result)
                    });
                }
            }
            return BadRequest(new
            {
                Title = "Get notification types failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationTypeById(int id)
        {
            if (ModelState.IsValid && id > 0)
            {
                var existedNotificationType = await _notificationTypeService.GetById(id);
                if(existedNotificationType is null)
                {
                    return NotFound(new
                    {
                        Title = "Notification type not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<NotificationTypeResponseVM>(existedNotificationType)
                });
            }
            return BadRequest(new
            {
                Title = "Get notification type failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
