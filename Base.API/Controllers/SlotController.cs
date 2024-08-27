using AutoMapper;
using Base.API.Service;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlotController : ControllerBase
    {
        private readonly ISlotService _slotService;
        private readonly IMapper _mapper;
        private readonly HangfireService _hangFireService;
        public SlotController(ISlotService slotService, IMapper mapper, HangfireService hangfireService)
        {
            _slotService = slotService; 
            _mapper = mapper;
            _hangFireService = hangfireService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSlots()
        {
            var slots = await _slotService.Get();
            return Ok(_mapper.Map<IEnumerable<SlotResponse>>(slots));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSlotById(int id)
        {
            if(ModelState.IsValid && id > 0)
            {
                var existedSlot = await _slotService.GetById(id);
                if(existedSlot is null)
                {
                    return NotFound(new
                    {
                        Title = "Slot not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<SlotResponseVM>(existedSlot)
                });
            }
            return BadRequest(new
            {
                Title = "Get slot information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewSlot([FromBody]CreateSlotVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _slotService.Create(resource);
                if (result.IsSuccess)
                {
                    // Set job for created slot
                    _hangFireService.SetASlotProgress(result.Result!.SlotID, result.Result.StartTime, result.Result.Endtime);

                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<SlotResponseVM>(result.Result)
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
                Title = "Create slot failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSlot(int id, [FromBody] SlotVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _slotService.Update(resource, id);
                if (result.IsSuccess)
                {
                    // Set job for updated slot
                    _hangFireService.SetASlotProgress(result.Result!.SlotID, result.Result.StartTime, result.Result.Endtime);

                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<SlotResponseVM>(result.Result)
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
                Title = "Update slot failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            if (ModelState.IsValid && id > 0)
            {
                var result = await _slotService.Delete(id);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        result.Title
                    });
                }
                return BadRequest(new
                {
                    result.Title,
                    result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Delete slot failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
