using FAPCL.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class BookingController : ControllerBase
    {
        private readonly BookClassRoomContext _context;
        private readonly UserManager<AspNetUser> _userManager;

        public BookingController(BookClassRoomContext context, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("{roomId}/{slotId}")]
        public IActionResult GetBookingDetails(int roomId, int slotId, DateTime selectedDate)
        {
            var roomDetails = _context.Rooms
                .Where(r => r.RoomId == roomId)
                .Select(r => new Room
                {
                    RoomName = r.RoomName,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity
                })
                .FirstOrDefault();

            var slotDetails = _context.Slots.FirstOrDefault(s => s.SlotId == slotId);

            return Ok(new { RoomDetails = roomDetails, SlotDetails = slotDetails });
        }

        [HttpPost]
        public IActionResult CreateBooking([FromBody] BookingRequest request)
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
            _context.SaveChanges();

            return Ok(new { Message = "Booking Successfully!", BookingId = newBooking.BookingId });
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetBookingDetails(string userId, bool isAdmin,
            int currentPage = 1, string searchQuery = "")
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
            _context.SaveChanges();

            if (isAdmin)
            {
                var allBookingsQuery = _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.Slot)
                    .Include(b => b.User);

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    allBookingsQuery = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Booking, AspNetUser>)allBookingsQuery.Where(b => b.Room.RoomName.Contains(searchQuery));
                }

                int total = await allBookingsQuery.CountAsync();
                int totalPages = (int)Math.Ceiling(total / 10.0);
                currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

                var allBookings = await allBookingsQuery
                    .Skip((currentPage - 1) * 10)
                    .Take(10)
                    .ToListAsync();

                return Ok(new { AllBookings = allBookings, CurrentPage = currentPage, TotalPages = totalPages });
            }
            else
            {
                var confirmedQuery = _context.Bookings
                    .Where(b => b.UserId == userId && b.Status == "Confirmed")
                    .Include(b => b.Room)
                    .Include(b => b.Slot);

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    confirmedQuery = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Booking, Slot>)confirmedQuery.Where(b => b.Room.RoomName.Contains(searchQuery));
                }

                var confirmedBookings = await confirmedQuery.ToListAsync();

                var completeQuery = _context.Bookings
                    .Where(b => b.UserId == userId && (b.Status == "Completed" || b.Status == "Cancelled"))
                    .Include(b => b.Room)
                    .Include(b => b.Slot);

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    completeQuery = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Booking, Slot>)completeQuery.Where(b => b.Room.RoomName.Contains(searchQuery));
                }

                int total = await completeQuery.CountAsync();
                int totalPages = (int)Math.Ceiling(total / 6.0);
                currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

                var completeBookings = await completeQuery
                    .Skip((currentPage - 1) * 6)
                    .Take(6)
                    .ToListAsync();

                return Ok(new
                {
                    ConfirmedBookings = confirmedBookings,
                    CompleteBookings = completeBookings,
                    CurrentPage = currentPage,
                    TotalPages = totalPages
                });
            }
        }

        [HttpPost("cancel")]
        public IActionResult CancelBooking([FromBody] CancelBookingRequest request)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == request.BookingId);
            if (booking == null) return NotFound();

            if (booking.Status == "Confirmed")
            {
                booking.Status = "Cancelled";
                _context.Bookings.Update(booking);
                _context.SaveChanges();
            }

            return Ok(new { Message = "Booking cancelled successfully" });
        }
    }

    public class BookingRequest
    {
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public DateTime SelectedDate { get; set; }
        public string UserId { get; set; }
        public string Purpose { get; set; }
    }

    public class CancelBookingRequest
    {
        public int BookingId { get; set; }
    }
}
