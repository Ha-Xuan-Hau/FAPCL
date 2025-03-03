using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookClassRoom.Models;
using Microsoft.AspNetCore.SignalR;
using BookClassRoom.Hubs;

namespace BookClassRoom.Pages.ClassroomManagement
{
    public class IndexModel : PageModel
    {
        private readonly BookClassRoomContext _context;
        private readonly IHubContext<SignalRServer> _hubContext;
        public IndexModel(BookClassRoomContext context, IHubContext<SignalRServer> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IList<Room> Room { get; set; } = default!;
        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 6;

        [BindProperty(SupportsGet = true)]
        public int? RoomTypeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasProjector { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasSoundSystem { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoomName { get; set; }

        public async Task OnGetAsync(int currentPage = 1)
        {
            RoomTypes = await _context.RoomTypes.ToListAsync();

            var query = _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Bookings)
                .ThenInclude(b => b.Slot)
                .AsQueryable();

            if (!string.IsNullOrEmpty(RoomName))
            {
                query = query.Where(r => EF.Functions.Like(r.RoomName, $"%{RoomName}%"));
            }

            if (RoomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == RoomTypeId.Value);
            }

            if (HasProjector.HasValue)
            {
                query = query.Where(r => r.HasProjector == HasProjector.Value);
            }

            if (HasSoundSystem.HasValue)
            {
                query = query.Where(r => r.HasSoundSystem == HasSoundSystem.Value);
            }

            var totalRooms = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRooms / (double)PageSize);
            CurrentPage = Math.Max(1, Math.Min(currentPage, TotalPages));

            Room = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            UpdateRoomStatus();
            await _hubContext.Clients.All.SendAsync("LoadRoom");
        }

        private void UpdateRoomStatus()
        {
            var currentTime = DateTime.Now;
            var currentTimeOfDay = currentTime.TimeOfDay;

            foreach (var room in Room)
            {
                var roomBookings = room.Bookings
                    .Where(b => b.Status == "Confirmed")
                    .ToList();

                bool isInUse = false;
                bool isFutureBooking = false;

                foreach (var booking in roomBookings)
                {
                    var slot = booking.Slot;

                    if (booking.SlotBookingDate.Date > currentTime.Date ||
                        (booking.SlotBookingDate.Date == currentTime.Date && slot.StartTime > currentTimeOfDay))
                    {
                        isFutureBooking = true;
                        break;
                    }

                    if (booking.SlotBookingDate.Date == currentTime.Date &&
                        currentTimeOfDay >= slot.StartTime && currentTimeOfDay <= slot.EndTime)
                    {
                        isInUse = true;
                        break;
                    }
                }

                if (isInUse)
                {
                    room.Status = "InUse";
                }
                else if (isFutureBooking)
                {
                    room.Status = "Booked";
                }
                else
                {
                    room.Status = "Available";
                }
            }
        }

        public async Task<IActionResult> OnPost(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return NotFound();
            }

            room.IsAction = !room.IsAction;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("LoadRoom");

            return RedirectToPage();
        }

    }
}
