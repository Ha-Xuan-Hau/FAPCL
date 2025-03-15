using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Services
{
    public interface IBookingService
    {
        Task<IActionResult> GetBookingDetails(int roomId, int slotId, DateTime selectedDate);
        Task<IActionResult> CreateBooking(BookingRequest request);
        Task<IActionResult> GetBookingDetails(string userId, bool isAdmin, int currentPage = 1, string searchQuery = "");
        Task<IActionResult> CancelBooking(CancelBookingRequest request);
        Task<IActionResult> GetAllBookings(); // Change return type to Task<IActionResult>
    }
}
