using Microsoft.AspNetCore.Mvc;

namespace FAPCL.DTO
{
    namespace FAPCL.DTO
    {
        public class ClassDetailDto
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public CourseDto Course { get; set; }
            public TeacherDto Teacher { get; set; }
        }

        public class CourseDto
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; }
            public string Description { get; set; }
            public int Credits { get; set; }
        }
    }

}
