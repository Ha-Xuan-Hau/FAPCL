namespace FAPCLClient.Model.DTOs
{
    public class SchedulingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ScheduleId { get; set; }
        public List<ScheduledExamDTO> ScheduledExams { get; set; }
    }
}
