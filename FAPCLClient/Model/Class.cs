using System;
using System.Collections.Generic;

namespace FAPCLClient.Model
{
    public partial class Class
    {
        public Class()
        {
            ClassSchedules = new HashSet<ClassSchedule>();
            StudentClasses = new HashSet<StudentClass>();
            Timetables = new HashSet<Timetable>();
        }

        public int ClassId { get; set; }
        public string ClassName { get; set; } = null!;
        public int CourseId { get; set; }
        public string TeacherId { get; set; } = null!;
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual Slot Slot { get; set; } = null!;
        public virtual AspNetUser Teacher { get; set; } = null!;
        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; }
        public virtual ICollection<StudentClass> StudentClasses { get; set; }
        public virtual ICollection<Timetable> Timetables { get; set; }
    }
}
