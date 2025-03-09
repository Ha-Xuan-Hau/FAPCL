using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Services
{
    public interface IBookingService
    {
        Task<IActionResult> GetAllBookings();
        Task<IActionResult> GetBookingDetails(int roomId, int slotId, DateTime selectedDate);
        Task<IActionResult> CreateBooking(BookingRequest request);
        Task<IActionResult> GetBookingDetails(string userId, bool isAdmin, int currentPage, string searchQuery);
        Task<IActionResult> CancelBooking(CancelBookingRequest request);
    }
}
