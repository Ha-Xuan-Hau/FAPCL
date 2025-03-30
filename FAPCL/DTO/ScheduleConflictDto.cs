using Microsoft.AspNetCore.Mvc;

namespace FAPCL.DTO
{
    public class ScheduleConflictDto
    {
        public int ClassId { get; set; }
        public string DayOfWeek { get; set; }
        public int SlotId { get; set; }
    }
}
