using FAPCL.Model;

namespace FAPCL.Services;

public interface IRoomTypeService
{
    Task<IEnumerable<RoomType>> GetRoomTypes();
    Task<RoomType?> AddRoomType(RoomType roomType);
    Task<RoomType?> UpdateRoomType(RoomType roomType);
}