using FAPCL.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace FAPCL.Services
{
    public class RoomService : IRoomService
    {
        private readonly BookClassRoomContext _context;

        public RoomService(BookClassRoomContext context)
        {
            _context = context;
        }

        public IActionResult GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector, bool? hasSoundSystem, int currentPage = 1)
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

            return new OkObjectResult(response);
        }

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

            if (existingBooking != null) return new OkObjectResult(false);

            if (selectedDate.Date == currentDate)
            {
                var slot = _context.Slots.FirstOrDefault(s => s.SlotId == slotId);
                return new OkObjectResult(slot != null && slot.StartTime > currentTime);
            }

            return new OkObjectResult(true);
        }

        public async Task<IEnumerable<Room>> getAllRoom()
        {
            return await _context.Rooms.ToListAsync();
        }

        public async Task<Room?> getRoomById(int roomId)
        {
            return await _context.Rooms.FindAsync(roomId);
        }

        public async Task<Room?> addRoom(Room room)
        {
            await _context.Rooms.AddAsync(room);
            _context.SaveChanges();
            return room;
        }

        public async Task<Room?> updateRoom(int roomId, Room room)
        {
            var roomToUpdate = await _context.Rooms.FindAsync(roomId);
            if (roomToUpdate == null)
            {
                return null;
            }

            _context.Entry(roomToUpdate).CurrentValues.SetValues(room);
            await _context.SaveChangesAsync();
            return roomToUpdate;
        }

        public async Task<Room?> deleteRoom(int roomId)
        {
            var roomToDelete = await _context.Rooms.FindAsync(roomId);
            if (roomToDelete == null)
            {
                return null;
            }

            _context.Rooms.Remove(roomToDelete);
            await _context.SaveChangesAsync();
            return roomToDelete;
        }
    }
}