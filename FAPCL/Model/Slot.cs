using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class Slot
    {
        public Slot()
        {
            Bookings = new HashSet<Booking>();
            ClassSchedules = new HashSet<ClassSchedule>();
            ExamSchedules = new HashSet<ExamSchedule>();
            Exams = new HashSet<Exam>();
            Timetables = new HashSet<Timetable>();
        }

        public int SlotId { get; set; }
        public string SlotName { get; set; } = null!;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; }
        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        public virtual ICollection<Exam> Exams { get; set; }
        public virtual ICollection<Timetable> Timetables { get; set; }
    }
}
