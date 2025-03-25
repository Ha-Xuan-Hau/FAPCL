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

        public async Task<Booking?> GetBookingDetails(int roomId, int? slotId, DateTime? selectedDate)
        {
            var query = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Slot)
                .Where(b => b.RoomId == roomId);

            if (slotId.HasValue)
            {
                query = query.Where(b => b.SlotId == slotId.Value);
            }

            if (selectedDate.HasValue)
            {
                query = query.Where(b => b.SlotBookingDate.Date == selectedDate.Value.Date);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Booking?> CreateBooking(BookingRequest request, string userId)
        {
            var newBooking = new Booking
            {
                RoomId = request.RoomId,
                SlotId = request.SlotId,
                SlotBookingDate = request.SelectedDate,
                BookingDate = DateTime.Now,
                UserId = userId,
                Purpose = request.Purpose,
                Status = "Confirmed"
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return newBooking;
        }

        public async Task<IEnumerable<Booking>> GetBookingDetails(string userId, bool isAdmin, int currentPage = 1, string searchQuery = "")
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

            IQueryable<Booking> query;

            query = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Slot)
                .Include(b => b.User);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(b => b.Room.RoomName.Contains(searchQuery));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingCompleteds(string userId, int currentPage, string searchQuery)
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

            IQueryable<Booking> query;

            query = _context.Bookings
                .Where(b => b.UserId == userId && b.Status == "Completed")
                .Include(b => b.Room)
                .Include(b => b.Slot);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(b => b.Room.RoomName.Contains(searchQuery));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingConfirmeds(string userId, string searchQuery)
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

            IQueryable<Booking> query;

            query = _context.Bookings
                .Where(b => b.UserId == userId  && b.Status == "Confirmed")
                .Include(b => b.Room)
                .Include(b => b.Slot);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(b => b.Room.RoomName.Contains(searchQuery));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> CancelBooking(Booking booking)
        {
            if (booking.Status == "Confirmed")
            {
                booking.Status = "Cancelled";
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Booking>> GetAllBookings()
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Slot)
                .ToListAsync();
        }
    }
}
