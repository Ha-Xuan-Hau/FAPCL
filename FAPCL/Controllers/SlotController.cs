using FAPCL.Services;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SlotController : ControllerBase
{
    private readonly ISlotService _slotService;

    public SlotController(ISlotService slotService)
    {
        _slotService = slotService;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> GetAllSlots()
    {
        var result = await _slotService.GetAllSlots();

        if (result == null || !result.Any())
        {
            return NotFound("No slots found");
        }
        return Ok(result);
    }
    
    [HttpGet("{slotId}")]
    public async Task<IActionResult> GetSlotById(int slotId)
    {
        var result = await _slotService.GetSlotById(slotId);

        return result != null ? Ok(result) : NotFound($"Slot with ID {slotId} not found");
    }
}