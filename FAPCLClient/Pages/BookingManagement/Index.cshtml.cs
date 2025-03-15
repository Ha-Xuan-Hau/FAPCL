using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookClassRoom.Pages.BookingManagement
{
    public class IndexModel : PageModel
    {
        private readonly BookClassRoomContext _context;

        public IndexModel(BookClassRoomContext context)
        {
            _context = context;
        }

        public List<Room> FilteredRooms { get; set; } = new List<Room>();
        public List<Slot> Slots { get; set; } = new List<Slot>();
        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.Now.Date;

        [BindProperty(SupportsGet = true)]
        public int? RoomTypeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasProjector { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasSoundSystem { get; set; }
        public HashSet<(int RoomId, int SlotId)> BookedSlots { get; set; } = new HashSet<(int RoomId, int SlotId)>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 6;
        public void OnGet(int currentPage = 1)
        {
            if (SelectedDate < DateTime.Now.Date)
            {
                SelectedDate = DateTime.Now.Date;
            }

            RoomTypes = _context.RoomTypes.ToList();

            Slots = _context.Slots.ToList();
            var filteredRoomsQuery = _context.Rooms
                .Where(r => r.IsAction == true)
                .Where(r => !RoomTypeId.HasValue || r.RoomTypeId == RoomTypeId)
                .Where(r => !HasProjector.HasValue || r.HasProjector == HasProjector.Value)
                .Where(r => !HasSoundSystem.HasValue || r.HasSoundSystem == HasSoundSystem.Value);
            int totalRooms = filteredRoomsQuery.Count();
            TotalPages = (int)Math.Ceiling(totalRooms / (double)PageSize);

            CurrentPage = currentPage < 1 ? 1 : currentPage > TotalPages ? TotalPages : currentPage;

            FilteredRooms = filteredRoomsQuery
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public bool IsSlotAvailable(int roomId, int slotId, DateTime selectedDate)
        {
            var currentTime = DateTime.Now.TimeOfDay; 
            var currentDate = DateTime.Now.Date; 

            var existingBooking = _context.Bookings
                .Where(b => b.RoomId == roomId
                            && b.SlotId == slotId
                            && b.SlotBookingDate == selectedDate
                            && b.Status == "Confirmed")
                .FirstOrDefault();

            if (existingBooking != null)
            {
                return false; 
            }

            if (selectedDate.Date == currentDate)
            {
                var slot = _context.Slots.FirstOrDefault(s => s.SlotId == slotId);
                if (slot != null && slot.StartTime > currentTime)
                {
                    return true; 
                }

                return false; 
            }

            return true;
        }

    }
}