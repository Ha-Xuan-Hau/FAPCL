using FAPCL.Model;
using Microsoft.EntityFrameworkCore;

namespace FAPCL.Services;

public class RoomTypeService: IRoomTypeService
{
    private readonly BookClassRoomContext _context;

    public RoomTypeService(BookClassRoomContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<RoomType>> GetRoomTypes()
    {
        return await _context.RoomTypes.ToListAsync();
    }
    
    public async Task<RoomType?> AddRoomType(RoomType roomType)
    {
        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();
        return roomType;
    }
    
    public async Task<RoomType?> UpdateRoomType(RoomType roomType)
    {
        var existingRoomType = await _context.RoomTypes.FindAsync(roomType.RoomTypeId);
        if (existingRoomType != null)
        {
            roomType.RoomTypeId = existingRoomType.RoomTypeId;
            existingRoomType.RoomType1 = roomType.RoomType1;
            existingRoomType.Description = roomType.Description;
            await _context.SaveChangesAsync();
        }
        return existingRoomType;
    }
}