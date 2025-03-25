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

        public async Task<(IEnumerable<Room> Rooms, int TotalPages)> GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector, bool? hasSoundSystem, int currentPage = 1)
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

            int totalRooms = await filteredRoomsQuery.CountAsync();
            int pageSize = 6;
            int totalPages = (int)Math.Ceiling(totalRooms / (double)pageSize);
            currentPage = Math.Clamp(currentPage, 1, totalPages);

            var rooms = await filteredRoomsQuery
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (rooms, totalPages);
        }

        public async Task<bool> CheckSlotAvailability(int roomId, int slotId, DateTime selectedDate)
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

        public async Task<IEnumerable<Room>> GetAllRooms()
        {
            return await _context.Rooms.ToListAsync();
        }

        public async Task<Room?> GetRoomById(int roomId)
        {
            return await _context.Rooms.FindAsync(roomId);
        }

        public async Task<Room?> AddRoom(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> UpdateRoom(int roomId, Room room)
        {
            var roomToUpdate = await _context.Rooms.FindAsync(roomId);
            if (roomToUpdate == null) return null;

            _context.Entry(roomToUpdate).CurrentValues.SetValues(room);
            await _context.SaveChangesAsync();
            return roomToUpdate;
        }

        public async Task<bool> DeleteRoom(int roomId)
        {
            var roomToDelete = await _context.Rooms.FindAsync(roomId);
            if (roomToDelete == null) return false;

            _context.Rooms.Remove(roomToDelete);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}