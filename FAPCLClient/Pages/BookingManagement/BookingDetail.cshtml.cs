using BookClassRoom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BookClassRoom.Pages.BookingManagement
{
    public class BookingDetailModel : PageModel
    {
        private readonly BookClassRoomContext _context;
        private readonly UserManager<AspNetUser> _userManager;

        public BookingDetailModel(BookClassRoomContext context, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            ConfirmedBookings = new List<Booking>();
            CompleteBookings = new List<Booking>();
            AllBookings = new List<Booking>();
            Rooms = new List<Room>();
            SearchQuery = string.Empty;
        }

        public string UserId { get; set; }
        public bool IsAdmin { get; set; }
        public List<Booking> ConfirmedBookings { get; set; }
        public List<Booking> CompleteBookings { get; set; }
        public List<Booking> AllBookings { get; set; }
        public List<Room> Rooms { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 6;
        public const int PageSize2 = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public async Task OnGet(int currentPage = 1)
        {
            UserId = _userManager.GetUserId(User);
            IsAdmin = User.IsInRole("Admin");
            var currentTime = DateTime.Now;
            var currentTimeOfDay = currentTime.TimeOfDay;

            IQueryable<Booking> bookingsQuery = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Slot)
                .Include(b => b.User);

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                bookingsQuery = bookingsQuery.Where(b => b.Room.RoomName.Contains(SearchQuery));
            }

            var bookings = _context.Bookings
                .Where(b => b.SlotBookingDate <= currentTime && b.Slot.EndTime < currentTimeOfDay && b.Status == "Confirmed")
                .ToList();

            foreach (var booking in bookings)
            {
                booking.Status = "Completed";
            }
            _context.SaveChanges();

            if (IsAdmin)
            {
                IQueryable<Booking> allBookingsQuery = _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.Slot)
                    .Include(b => b.User);

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    allBookingsQuery = allBookingsQuery.Where(b => b.Room.RoomName.Contains(SearchQuery));
                }

                var allBooking = await allBookingsQuery.CountAsync();
                TotalPages = (int)Math.Ceiling(allBooking / (double)PageSize2);
                CurrentPage = Math.Max(1, Math.Min(currentPage, TotalPages));

                AllBookings = await allBookingsQuery
                    .Skip((CurrentPage - 1) * PageSize2)
                    .Take(PageSize2)
                    .ToListAsync();
            }
            else
            {
                IQueryable<Booking> confirmedQuery = _context.Bookings
                    .Where(b => b.UserId == UserId && b.Status == "Confirmed")
                    .Include(b => b.Room)
                    .Include(b => b.Slot);

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    confirmedQuery = confirmedQuery.Where(b => b.Room.RoomName.Contains(SearchQuery));
                }

                ConfirmedBookings = await confirmedQuery.ToListAsync();

                IQueryable<Booking> completeQuery = _context.Bookings
                    .Where(b => b.UserId == UserId && (b.Status == "Completed" || b.Status == "Cancelled"))
                    .Include(b => b.Room)
                    .Include(b => b.Slot);

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    completeQuery = completeQuery.Where(b => b.Room.RoomName.Contains(SearchQuery));
                }

                var total = await completeQuery.CountAsync();
                TotalPages = (int)Math.Ceiling(total / (double)PageSize);
                CurrentPage = Math.Max(1, Math.Min(currentPage, TotalPages));

                CompleteBookings = await completeQuery
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }
        }

        public IActionResult OnPostCancelBooking(int bookingId)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == "Confirmed")
            {
                booking.Status = "Cancelled";
                _context.Bookings.Update(booking);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}
    
