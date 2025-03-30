namespace FAPCL.DTO
{
    public class ClassDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int CourseId { get; set; } 
        public string CourseName { get; set; }

        public string TeacherId { get; set; } 
        public string TeacherName { get; set; }

        public int RoomId { get; set; }  
        public string RoomName { get; set; }
    }
}
