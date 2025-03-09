using FAPCL.DTO;
using FAPCL.Model;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> getAllRoom();
        Task<Room?> getRoomById(int roomId);
        Task<Room?> addRoom(Room room);
        Task<Room?> updateRoom(int roomId, Room room);
        Task<Room?> deleteRoom(int roomId);
        IActionResult GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector, bool? hasSoundSystem, int currentPage);
        IActionResult CheckSlotAvailability(int roomId, int slotId, DateTime selectedDate);
    }
}
