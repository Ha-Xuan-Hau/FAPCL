using Microsoft.AspNetCore.Mvc;

namespace FAPCL.DTO
{
    public class ClassScheduleDto
    {
        public int ClassId { get; set; }
        public int SlotId { get; set; }
        public string DayOfWeek { get; set; }
    }
}
