namespace FAPCLClient.Model.DTOs
{
    public class ScheduledExamDTO
    {
        public int ExamId { get; set; }

        public String ExamName { get; set; }
        public string CourseName { get; set; }
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
