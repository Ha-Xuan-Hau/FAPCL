using FAPCL.DTO;
using FAPCL.Model;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllBookings();
        Task<Booking> GetBookingDetails(int roomId, int slotId, DateTime selectedDate);
        Task<Booking> CreateBooking(BookingRequest request);
        Task<IEnumerable<Booking>> GetBookingDetails(string userId, bool isAdmin, int currentPage, string searchQuery);
        Task<bool> CancelBooking(CancelBookingRequest request);
    }
}
