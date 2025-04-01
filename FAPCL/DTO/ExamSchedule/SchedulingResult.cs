namespace FAPCL.DTO.ExamSchedule
{
    public class SchedulingResult
    {
         public bool Success { get; set; }
        public string Message { get; set; }
        public int? ScheduleId { get; set; }
        public List<ScheduledExamInfo> ScheduledExams { get; set; }
    }
}
