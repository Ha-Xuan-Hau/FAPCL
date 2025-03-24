using FAPCL.DTO;
using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController(IBookingService bookingService) : ControllerBase
    {
        [HttpGet("{roomId}/{slotId}")]
        public async Task<IActionResult> GetBookingDetails(int roomId, int slotId, DateTime selectedDate)
        {
            var result = await bookingService.GetBookingDetails(roomId, slotId, selectedDate);
            return result != null ? Ok(result) : NotFound("Booking details not found");
        }
        
        [HttpPost("createBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest request)
        {
            var result = await bookingService.CreateBooking(request);
            return result != null ? Ok(result) : BadRequest("Failed to create booking");
        }
        
        [HttpGet("details")]
        public async Task<IActionResult> GetBookingDetails(int currentPage = 1, string searchQuery = "")
        {
            var isAdmin = true;
            var userId = 1.ToString();
            var result = await bookingService.GetBookingDetails(userId, isAdmin, currentPage, searchQuery);
            return Ok(result);
        }
        
        [HttpGet("completed")]
        public async Task<IActionResult> GetBookingCompleteds(int currentPage = 1, string searchQuery = "")
        {
            var isAdmin = true;
            var userId = 1.ToString();
            var result = await bookingService.GetBookingCompleteds(userId, currentPage, searchQuery);
            return Ok(result);
        }
        
        [HttpGet("confirmed")]
        public async Task<IActionResult> GetBookingConfirmeds(string searchQuery = "")
        {
            var isAdmin = true;
            var userId = 1.ToString();
            var result = await bookingService.GetBookingConfirmeds(userId, searchQuery);
            return Ok(result);
        }
        
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingRequest request)
        {
            Booking? booking = await bookingService.GetBookingDetails(request.BookingId);

            if (booking == null)
            {
                return NotFound("Booking not found");
            }

            bool isCanceled = await bookingService.CancelBooking(booking);
            return isCanceled ? Ok("Booking canceled successfully") : BadRequest("Failed to cancel booking");
        }
    }
}