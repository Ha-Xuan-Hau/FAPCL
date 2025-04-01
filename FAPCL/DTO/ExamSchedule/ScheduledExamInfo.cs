namespace FAPCL.DTO.ExamSchedule
{

    public class ScheduledExamInfo
    {
        public int ExamId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string ExamName { get; set; }
        public DateTime ExamDate { get; set; }
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int StudentCount { get; set; }
        public string TeacherName { get; set; }
    }

}