namespace FAPCL.DTO.ExamSchedule
{
    public class TeacherInfo
    {
        public string TeacherId { get; set; }
        public string TeacherName { get; set; }
    }

    public class StudentInfo
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
    }

    public class DetailedExamInfo
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public DateTime ExamDate { get; set; }
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public TeacherInfo Teacher { get; set; }
        public List<StudentInfo> Students { get; set; } = new List<StudentInfo>();
    }

        public class DetailedExamResult
    {
         public bool Success { get; set; }
        public string Message { get; set; }
        public int? ScheduleId { get; set; }
        public List<DetailedExamInfo> DetailedExam { get; set; }
    }
}
