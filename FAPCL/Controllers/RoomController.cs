using FAPCL.Services;
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
    }
}