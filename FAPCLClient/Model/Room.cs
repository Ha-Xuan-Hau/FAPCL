using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class Room
    {
        public Room()
        {
            Bookings = new HashSet<Booking>();
            Classes = new HashSet<Class>();
            ExamSchedules = new HashSet<ExamSchedule>();
            Exams = new HashSet<Exam>();
            RoomEquipments = new HashSet<RoomEquipment>();
            RoomStatusLogs = new HashSet<RoomStatusLog>();
        }

        public int RoomId { get; set; }
        public string RoomName { get; set; } = null!;
        public int Capacity { get; set; }
        public int RoomTypeId { get; set; }
        public bool? HasProjector { get; set; }
        public bool? HasSoundSystem { get; set; }
        public string? Status { get; set; }
        public bool? IsAction { get; set; }

        public virtual RoomType RoomType { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        public virtual ICollection<Exam> Exams { get; set; }
        public virtual ICollection<RoomEquipment> RoomEquipments { get; set; }
        public virtual ICollection<RoomStatusLog> RoomStatusLogs { get; set; }
    }
}
