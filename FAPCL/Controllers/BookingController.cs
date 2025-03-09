using FAPCL.DTO;
using FAPCL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [Authorize]
        [HttpGet("{roomId}/{slotId}")]
        public async Task<IActionResult> GetBookingDetails(int roomId, int slotId, DateTime selectedDate)
        {
            return await _bookingService.GetBookingDetails(roomId, slotId, selectedDate);
        }

        [Authorize]
        [HttpPost("createBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest request)
        {
            return await _bookingService.CreateBooking(request);
        }

        [Authorize]
        [HttpGet("details")]
        public async Task<IActionResult> GetBookingDetails(string userId, bool isAdmin, int currentPage = 1, string searchQuery = "")
        {
            return await _bookingService.GetBookingDetails(userId, isAdmin, currentPage, searchQuery);
        }

        [Authorize]
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingRequest request)
        {
            return await _bookingService.CancelBooking(request);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/list")]
        public async Task<IActionResult> GetAllBookings()
        {
            var listBookings = await _bookingService.GetAllBookings();
            return new OkObjectResult(listBookings);
        }
    }
}