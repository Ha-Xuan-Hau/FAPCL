using FAPCL.Model;
using Microsoft.EntityFrameworkCore;

namespace FAPCL.Services;

public class SlotService : ISlotService
{
    private readonly BookClassRoomContext _context;

    public SlotService(BookClassRoomContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Slot>> GetAllSlots()
    {
        return await _context.Slots.ToListAsync();
    }

    public async Task<Slot?> GetSlotById(int slotId)
    {
        return await _context.Slots.FindAsync(slotId);
    }
}