using System.ComponentModel.DataAnnotations;

namespace FAPCL.DTO.ExamSchedule
{
    public class CourseDTO
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;

        public string Description { get; set; } = null!;
    }
}