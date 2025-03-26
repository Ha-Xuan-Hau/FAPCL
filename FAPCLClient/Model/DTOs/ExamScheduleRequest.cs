namespace FAPCLClient.Model.DTOs
{
    public class ExamScheduleRequest
    {
        public string ExamName { get; set; }
        public List<int> CourseIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
