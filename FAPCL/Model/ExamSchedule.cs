using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class ExamSchedule
    {
        public int ExamScheduleId { get; set; }
        public int ExamId { get; set; }
        public string StudentId { get; set; } = null!;
        public string TeacherId { get; set; } = null!;
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public DateTime ExamDate { get; set; }

        public virtual Exam Exam { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual Slot Slot { get; set; } = null!;
        public virtual AspNetUser Student { get; set; } = null!;
        public virtual AspNetUser Teacher { get; set; } = null!;
    }
}
