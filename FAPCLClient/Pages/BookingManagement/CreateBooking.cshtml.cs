using BookClassRoom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookClassRoom.Pages.BookingManagement
{
    public class CreateBookingModel : PageModel
    {
        private readonly BookClassRoomContext _context;
        private readonly UserManager<AspNetUser> _userManager;

        [BindProperty(SupportsGet = true)]
        public int RoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SlotId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.Now.Date;
        public Room RoomDetails { get; set; } = new Room();
        public Slot SlotDetails { get; set; } = new Slot();
        public string UserId { get; set; }

        [BindProperty]
        public string Purpose { get; set; }
        public CreateBookingModel(BookClassRoomContext context, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            UserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            RoomDetails = _context.Rooms
               .Where(r => r.RoomId == RoomId)
               .Select(r => new Room
               {
                   RoomName = r.RoomName,
                   RoomType = r.RoomType,
                   Capacity = r.Capacity
               })
               .FirstOrDefault();
            SlotDetails = _context.Slots.FirstOrDefault(s => s.SlotId == SlotId);
            
            return Page();
        }
        public IActionResult OnPost()
        {
            UserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var newBooking = new Booking
            {
                RoomId = RoomId,
                SlotId = SlotId,
                SlotBookingDate = SelectedDate,
                BookingDate = DateTime.Now,
                UserId = UserId,
                Purpose = Purpose,
                Status = "Confirmed"
            };
            _context.Bookings.Add(newBooking);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Booking Successfully!";

            return RedirectToPage("/BookingManagement/Index");
        }
    }
}
