using FAPCL.DTO;
using FAPCL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        
        [HttpGet("{roomId}/{slotId}")]
        public async Task<IActionResult> GetBookingDetails(int roomId, int slotId, DateTime selectedDate)
        {
            var result = await _bookingService.GetBookingDetails(roomId, slotId, selectedDate);
            return result != null ? Ok(result) : NotFound("Booking details not found");
        }
        
        [HttpPost("createBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest request)
        {
            var result = await _bookingService.CreateBooking(request);
            return result != null ? Ok(result) : BadRequest("Failed to create booking");
        }
        
        [HttpGet("details")]
        public async Task<IActionResult> GetBookingDetails(string userId, bool isAdmin, int currentPage = 1, string searchQuery = "")
        {
            var result = await _bookingService.GetBookingDetails(userId, isAdmin, currentPage, searchQuery);
            return Ok(result);
        }
        
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingRequest request)
        {
            bool isCanceled = await _bookingService.CancelBooking(request);
            return isCanceled ? Ok("Booking canceled successfully") : BadRequest("Failed to cancel booking");
        }

        [HttpGet("admin/list")]
        public async Task<IActionResult> GetAllBookings()
        {
            var listBookings = await _bookingService.GetAllBookings();
            return Ok(listBookings);
        }
    }
}