using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotifications(
            [FromQuery] int startPage,
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] bool? read,
            [FromQuery] Guid? userId,
            [FromQuery] int? notificationTypeId)
        {
            if (ModelState.IsValid)
            {
                var result = await _notificationService.GetAll(startPage, endPage, quantity, read, userId, notificationTypeId);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<IEnumerable<NotificationResponseVM>>(result.Result)
                    });
                }
            }
            return BadRequest(new
            {
                Title = "Get notifications failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            if (ModelState.IsValid && id > 0)
            {
                var existedNotification = await _notificationService.GetById(id);
                if(existedNotification is null)
                {
                    return NotFound(new
                    {
                        Title = "Notification not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<NotificationResponseVM>(existedNotification)
                });
            }
            return BadRequest(new
            {
                Title = "Get notification information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
