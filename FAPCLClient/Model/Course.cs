using System;
using System.Collections.Generic;

namespace FAPCLClient.Model
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
            Exams = new HashSet<Exam>();
        }

        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string? Description { get; set; }
        public int Credits { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Exam> Exams { get; set; }
    }
}
