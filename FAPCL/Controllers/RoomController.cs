using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public async Task<IActionResult> GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector, bool? hasSoundSystem, int currentPage = 1)
        {
            var result = await _roomService.GetRooms(selectedDate, roomTypeId, hasProjector, hasSoundSystem, currentPage);
            
            if (result.Rooms == null || !result.Rooms.Any())
            {
                return NotFound("No rooms found");
            }

            return Ok(new { result.Rooms, result.TotalPages });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckSlotAvailability(int roomId, int slotId, DateTime selectedDate)
        {
            var isAvailable = await _roomService.CheckSlotAvailability(roomId, slotId, selectedDate);
            return Ok(new { roomId, slotId, selectedDate, isAvailable });
        }
        
        [HttpGet("admin/room")]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomService.GetAllRooms();
            return rooms.Any() ? Ok(rooms) : NotFound("No rooms available");
        }
        
        [HttpGet("admin/room/{roomId}")]
        public async Task<IActionResult> GetRoomById(int roomId)
        {
            var room = await _roomService.GetRoomById(roomId);
            return room != null ? Ok(room) : NotFound($"Room with ID {roomId} not found");
        }
        
        [HttpPut("admin/room/{roomId}")]
        public async Task<IActionResult> UpdateRoom(int roomId, [FromBody] Room room)
        {
            var updatedRoom = await _roomService.UpdateRoom(roomId, room);
            return updatedRoom != null ? Ok(updatedRoom) : NotFound($"Room with ID {roomId} not found");
        }
        
        [HttpPost("admin/room/add")]
        public async Task<IActionResult> AddRoom([FromBody] Room room)
        {
            var newRoom = await _roomService.AddRoom(room);
            if (newRoom == null)
            {
                return BadRequest("Failed to add room");
            }
            return CreatedAtAction(nameof(GetRoomById), new { roomId = newRoom.RoomId }, newRoom);
        }
        
        [HttpDelete("admin/room/{roomId}")]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            var isDeleted = await _roomService.DeleteRoom(roomId);
            return isDeleted ? Ok($"Room {roomId} deleted successfully") : NotFound($"Room {roomId} not found");
        }

        [HttpGet("admin/room/{roomId}/{isAction}")]
        public async Task<IActionResult> ToggleRoomAction(int roomId, bool isAction)
        {
            var room = await _roomService.ToggleRoomAction(roomId, isAction);
            return room != null ? Ok(room) : NotFound($"Room with ID {roomId} not found");
        }
    }
}