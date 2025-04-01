namespace FAPCLClient.Model.DTOs
{
    public class ExamListItem
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public DateTime ExamDate { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomName { get; set; }
        public int StudentCount { get; set; }
    }

}
