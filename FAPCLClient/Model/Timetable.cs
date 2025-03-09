using System;
using System.Collections.Generic;

namespace FAPCLClient.Model
{
    public partial class Timetable
    {
        public int TimetableId { get; set; }
        public string StudentId { get; set; } = null!;
        public int ClassId { get; set; }
        public int SlotId { get; set; }
        public string DayOfWeek { get; set; } = null!;

        public virtual Class Class { get; set; } = null!;
        public virtual Slot Slot { get; set; } = null!;
        public virtual AspNetUser Student { get; set; } = null!;
    }
}
