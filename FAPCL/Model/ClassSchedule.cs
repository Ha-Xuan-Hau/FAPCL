using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class ClassSchedule
    {
        public int ClassScheduleId { get; set; }
        public int ClassId { get; set; }
        public int SlotId { get; set; }
        public string DayOfWeek { get; set; } = null!;

        public virtual Class Class { get; set; } = null!;
        public virtual Slot Slot { get; set; } = null!;
    }
}
