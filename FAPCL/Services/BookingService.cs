using FAPCL.DTO;
using FAPCL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FAPCL.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookClassRoomContext _context;
        private readonly UserManager<AspNetUser> _userManager;

        public BookingService(BookClassRoomContext context, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> GetBookingDetails(int roomId, int slotId, DateTime selectedDate)
        {
            var roomDetails = await _context.Rooms
                .Where(r => r.RoomId == roomId)
                .Select(r => new Room
                {
                    RoomName = r.RoomName,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity
                })
                .FirstOrDefaultAsync();

            var slotDetails = await _context.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);

            return new OkObjectResult(new { RoomDetails = roomDetails, SlotDetails = slotDetails });
        }

        public async Task<IActionResult> CreateBooking(BookingRequest request)
        {
            var newBooking = new Booking
            {
                RoomId = request.RoomId,
                SlotId = request.SlotId,
                SlotBookingDate = request.SelectedDate,
                BookingDate = DateTime.Now,
                UserId = request.UserId,
                Purpose = request.Purpose,
                Status = "Confirmed"
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { Message = "Booking Successfully!", BookingId = newBooking.BookingId });
        }

        public async Task<IActionResult> GetBookingDetails(string userId, bool isAdmin, int currentPage = 1, string searchQuery = "")
        {
            var currentTime = DateTime.Now;
            var currentTimeOfDay = currentTime.TimeOfDay;

            var bookingsToUpdate = _context.Bookings
                .Where(b => b.SlotBookingDate <= currentTime
                    && b.Slot.EndTime < currentTimeOfDay
                    && b.Status == "Confirmed")
                .ToList();

            foreach (var booking in bookingsToUpdate)
            {
                booking.Status = "Completed";
            }
            await _context.SaveChangesAsync();

            if (isAdmin)
            {
                var allBookingsQuery = _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.Slot)
                    .Include(b => b.User);

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    allBookingsQuery = allBookingsQuery.Where(b => b.Room.RoomName.Contains(searchQuery))
                        .Cast<Booking>()
                        .AsQueryable()
                        .Include(b => b.Room)
                        .Include(b => b.Slot)
                        .Include(b => b.User);
                }

                int total = await allBookingsQuery.CountAsync();
                int totalPages = (int)Math.Ceiling(total / 10.0);
                currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

                var allBookings = await allBookingsQuery
                    .Skip((currentPage - 1) * 10)
                    .Take(10)
                    .ToListAsync();

                return new OkObjectResult(new { AllBookings = allBookings, CurrentPage = currentPage, TotalPages = totalPages });
            }
            else
            {
                var confirmedQuery = _context.Bookings
                    .Where(b => b.UserId == userId && b.Status == "Confirmed")
                    .Include(b => b.Room)
                    .Include(b => b.Slot);

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    confirmedQuery = confirmedQuery.Where(b => b.Room.RoomName.Contains(searchQuery))
                        .Cast<Booking>()
                        .AsQueryable()
                        .Include(b => b.Room)
                        .Include(b => b.Slot);
                }

                var confirmedBookings = await confirmedQuery.ToListAsync();

                var completeQuery = _context.Bookings
                    .Where(b => b.UserId == userId && (b.Status == "Completed" || b.Status == "Cancelled"))
                    .Include(b => b.Room)
                    .Include(b => b.Slot);

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    completeQuery = completeQuery.Where(b => b.Room.RoomName.Contains(searchQuery))
                        .Cast<Booking>()
                        .AsQueryable()
                        .Include(b => b.Room)
                        .Include(b => b.Slot);
                }

                int total = await completeQuery.CountAsync();
                int totalPages = (int)Math.Ceiling(total / 6.0);
                currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

                var completeBookings = await completeQuery
                    .Skip((currentPage - 1) * 6)
                    .Take(6)
                    .ToListAsync();

                return new OkObjectResult(new
                {
                    ConfirmedBookings = confirmedBookings,
                    CompleteBookings = completeBookings,
                    CurrentPage = currentPage,
                    TotalPages = totalPages
                });
            }
        }

        public async Task<IActionResult> CancelBooking(CancelBookingRequest request)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == request.BookingId);
            if (booking == null) return new NotFoundResult();

            if (booking.Status == "Confirmed")
            {
                booking.Status = "Cancelled";
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
            }

            return new OkObjectResult(new { Message = "Booking cancelled successfully" });
        }

        public async Task<IEnumerable<Booking>> GetAllBookings()
        {
            return await _context.Bookings.ToListAsync();
        }
    }
}
