using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Services
{
    public interface IRoomService
    {
        IActionResult GetRooms(DateTime? selectedDate, int? roomTypeId, bool? hasProjector, bool? hasSoundSystem, int currentPage);
        IActionResult CheckSlotAvailability(int roomId, int slotId, DateTime selectedDate);
    }
}
