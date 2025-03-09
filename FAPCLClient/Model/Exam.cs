using System;
using System.Collections.Generic;

namespace FAPCLClient.Model
{
    public partial class Exam
    {
        public Exam()
        {
            ExamSchedules = new HashSet<ExamSchedule>();
        }

        public int ExamId { get; set; }
        public string ExamName { get; set; } = null!;
        public int CourseId { get; set; }
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public DateTime ExamDate { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual Slot Slot { get; set; } = null!;
        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
    }
}
