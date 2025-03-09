using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet("rooms")]
        public IActionResult GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector, bool? hasSoundSystem, int currentPage = 1)
        {
            return _roomService.GetRooms(selectedDate, roomTypeId, hasProjector, hasSoundSystem, currentPage);
        }

        [HttpGet("availability")]
        public IActionResult CheckSlotAvailability(int roomId, int slotId, DateTime selectedDate)
        {
            return _roomService.CheckSlotAvailability(roomId, slotId, selectedDate);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/admin/room")]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomService.getAllRoom();
            if (rooms == null || !rooms.Any())
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(rooms);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/admin/room/{roomId}")]
        public IActionResult GetRooms(int roomId)
        {
            var rooms = _roomService.getRoomById(roomId);
            if (rooms == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(rooms);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/admin/room/{roomId}")]
        public IActionResult UpdateRoom(int roomId, [FromBody] Room room)
        {
            var roomupdate = _roomService.updateRoom(roomId, room);
            if (roomupdate == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(roomupdate);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("/admin/room/add")]
        public IActionResult AddRoom([FromBody] Room room)
        {
            var roomAdd = _roomService.addRoom(room);
            if (roomAdd == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(room);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("/admin/room/{roomId}")]
        public IActionResult DeleteRoom(int roomId)
        {
            var roomDelete = _roomService.deleteRoom(roomId);
            if (roomDelete == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(roomId);
        }
    }
}