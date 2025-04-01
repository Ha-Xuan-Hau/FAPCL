using FAPCL.DTO;
using FAPCL.Model;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRooms();
        Task<Room?> GetRoomById(int roomId);
        Task<Room?> AddRoom(Room room);
        Task<Room?> UpdateRoom(int roomId, Room room);
        Task<bool> DeleteRoom(int roomId);
        Task<(IEnumerable<Room> Rooms, int TotalPages)> GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector, bool? hasSoundSystem, int currentPage);
        Task<bool> CheckSlotAvailability(int roomId, int slotId, DateTime selectedDate);
        Task<bool> ToggleRoomAction(int roomId, bool isAction);
    }
}
