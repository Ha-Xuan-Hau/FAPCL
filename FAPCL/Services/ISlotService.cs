using FAPCL.Model;

namespace FAPCL.Services;

public interface ISlotService
{
    Task<IEnumerable<Slot>> GetAllSlots();
    Task<Slot?> GetSlotById(int slotId);
}