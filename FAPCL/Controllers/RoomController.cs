using FAPCL.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public RoomController(BookClassRoomContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector,
            bool? hasSoundSystem, int currentPage = 1)
        {
            selectedDate ??= DateTime.Now.Date;
            if (selectedDate < DateTime.Now.Date)
            {
                selectedDate = DateTime.Now.Date;
            }

            var filteredRoomsQuery = _context.Rooms
                .Where(r => r.IsAction == true)
                .Where(r => !roomTypeId.HasValue || r.RoomTypeId == roomTypeId)
                .Where(r => !hasProjector.HasValue || r.HasProjector == hasProjector.Value)
                .Where(r => !hasSoundSystem.HasValue || r.HasSoundSystem == hasSoundSystem.Value);

            int totalRooms = filteredRoomsQuery.Count();
            int pageSize = 6;
            int totalPages = (int)Math.Ceiling(totalRooms / (double)pageSize);

            currentPage = currentPage < 1 ? 1 : currentPage > totalPages ? totalPages : currentPage;

            var rooms = filteredRoomsQuery
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                FilteredRooms = rooms,
                Slots = _context.Slots.ToList(),
                RoomTypes = _context.RoomTypes.ToList(),
                CurrentPage = currentPage,
                TotalPages = totalPages,
                SelectedDate = selectedDate
            };

            return Ok(response);
        }

        [HttpGet("availability")]
        public IActionResult CheckSlotAvailability(int roomId, int slotId, DateTime selectedDate)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var currentDate = DateTime.Now.Date;

            var existingBooking = _context.Bookings
                .Where(b => b.RoomId == roomId
                    && b.SlotId == slotId
                    && b.SlotBookingDate == selectedDate
                    && b.Status == "Confirmed")
                .FirstOrDefault();

            if (existingBooking != null) return Ok(false);

            if (selectedDate.Date == currentDate)
            {
                var slot = _context.Slots.FirstOrDefault(s => s.SlotId == slotId);
                return Ok(slot != null && slot.StartTime > currentTime);
            }

            return Ok(true);
        }
    }
}