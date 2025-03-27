using Microsoft.AspNetCore.Mvc;

namespace FAPCL.DTO
{
    public class ScheduleEntryDto
    {
        public DateTime Date { get; set; }        
        public string SlotName { get; set; }     
        public TimeSpan StartTime { get; set; }   
        public TimeSpan EndTime { get; set; }      
        public string ClassName { get; set; }     
        public string CourseName { get; set; }    
        public string TeacherName { get; set; }   
        public string RoomName { get; set; }
        public string DayOfWeek { get; set; }
        public int ClassId { get; set; }

    }
}
