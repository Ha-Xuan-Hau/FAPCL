using FAPCL.DTO;
using FAPCL.Model;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Services
{
    public interface IBookingService
    {
        Task<Booking?> GetBookingById(int bookingId);
        Task<IEnumerable<Booking>> GetAllBookings();
        Task<Booking?> GetBookingDetails(int roomId, int? slotId = null, DateTime? selectedDate = null);
        Task<Booking?> CreateBooking(BookingRequest request, string userId);
        Task<IEnumerable<Booking>> GetBookingDetails(string userId, bool isAdmin, int currentPage, string searchQuery);
        Task<IEnumerable<Booking>> GetBookingCompleteds(string userId, int currentPage, string searchQuery);
        Task<IEnumerable<Booking>> GetBookingConfirmeds(string userId, string searchQuery);
        Task<bool> CancelBooking(Booking? booking);
    }
}
