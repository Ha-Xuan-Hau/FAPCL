using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomTypeController : ControllerBase
{
    
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypeController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetRoomTypes()
    {
        var result = await _roomTypeService.GetRoomTypes();

        if (result == null || !result.Any())
        {
            return NotFound("No rooms found");
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddRoomType([FromBody] RoomType roomType)
    {
        var result = await _roomTypeService.AddRoomType(roomType);
        return result != null ? Ok(result) : BadRequest("Failed to add room type");
    }
    
    [HttpPut("{roomTypeId}")]
    public async Task<IActionResult> UpdateRoomType(int roomTypeId, [FromBody] RoomType roomType)
    {
        var updatedRoomType = await _roomTypeService.UpdateRoomType(roomType);
        return updatedRoomType != null ? Ok(updatedRoomType) : NotFound($"Room type with ID {roomTypeId} not found");
    }
}