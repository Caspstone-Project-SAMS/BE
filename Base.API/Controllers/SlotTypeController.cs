using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SlotTypeController : ControllerBase
{
    private readonly ISlotTypeService _slotTypeService;
    private readonly IMapper _mapper;

    public SlotTypeController(ISlotTypeService slotTypeService, IMapper mapper)
    {
        _slotTypeService = slotTypeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSlotType(
        [FromQuery] int startPage,
        [FromQuery] int endPage,
        [FromQuery] int quantity,
        [FromQuery] string? typeName,
        [FromQuery] string? description,
        [FromQuery] int? status,
        [FromQuery] int? sessionCount)
    {
        if (ModelState.IsValid)
        {
            var result = await _slotTypeService.GetAll(startPage, endPage, quantity, typeName, description, status, sessionCount);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title,
                    Result = _mapper.Map<IEnumerable<SlotTypeResponseVM>>(result.Result)
                });
            }
            return BadRequest(new
            {
                Title = "Get slot types falied",
                Errors = result.Errors
            });
        }
        return BadRequest(new
        {
            Title = "Get slot types failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSlotTypeById(int id)
    {
        if (ModelState.IsValid && id > 0)
        {
            var existedClass = await _slotTypeService.GetById(id);
            if (existedClass is null)
            {
                return NotFound(new
                {
                    Title = "Get slot type information failed",
                    Errors = new string[1] { "Slot type not found" }
                });
            }
            return Ok(new
            {
                Title = "Get slot type information successfully",
                Result = _mapper.Map<SlotTypeResponseVM>(existedClass)
            });
        }
        return BadRequest(new
        {
            Title = "Get slot type information failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSlotType([FromBody] SlotTypeVM resource)
    {
        if (ModelState.IsValid)
        {
            var result = await _slotTypeService.CreateSlotType(resource);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title,
                    Result = result.Result
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
            Title = "Create slot type failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSlotType(int id, [FromBody] SlotTypeVM resource)
    {
        if(ModelState.IsValid && id > 0) {
            var result = await _slotTypeService.UpdateSlotType(id, resource);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = result.Title,
                    Result = result.Result
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
            Title = "Update slot type failed",
            Errors = new string[1] { "Invalid input" }
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSlotType(int id)
    {
        if(id > 0)
        {
            var result = await _slotTypeService.DeleteSlotType(id);
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
            Title = "Delete slot type failed",
            Errors = new string[1] { "Invalid input" }
        });
    }
}
