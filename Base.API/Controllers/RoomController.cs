using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IMapper _mapper;
        public RoomController(IRoomService roomService, IMapper mapper)
        {
            _roomService = roomService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoom()
        {
            var rooms = await _roomService.GetAll();
            return Ok(_mapper.Map<IEnumerable<RoomResponse>>(rooms));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(RoomVM resource)
        {
            var result = await _roomService.Create(resource);
            if (result.IsSuccess)
            {
                return Ok("Create Room Successfully");
            }

            return BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRoom(RoomVM resource, int id)
        {
            var result = await _roomService.Update(resource, id);
            if (result.IsSuccess)
            {
                return Ok("Update Room Successfully");
            }

            return BadRequest(result);
        }
    }
}
