using AutoMapper;
using Base.Service.IService;
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
        public SlotController(ISlotService slotService, IMapper mapper)
        {
            _slotService = slotService; 
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetSlots()
        {
            var slots = await _slotService.Get();
            return Ok(_mapper.Map<IEnumerable<SlotResponse>>(slots));
        }


    }
}
