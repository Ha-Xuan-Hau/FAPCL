namespace FAPCL.DTO.ExamSchedule
{
    public class StudentExamScheduleDTO
    {
        public int ExamId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public DateTime ExamDate { get; set; }
        public string RoomName { get; set; }
        public string Time { get; set; }
        public string ExamType { get; set; }
    }
}
