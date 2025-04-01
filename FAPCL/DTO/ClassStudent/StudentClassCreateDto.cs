using System.ComponentModel.DataAnnotations;

namespace FAPCL.DTO.ExamSchedule
{
public class StudentClassCreateDto
    {
        public string StudentId { get; set; } = null!;
        public int ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Status { get; set; }  // Mặc định là "Enrolled" nếu không có giá trị
    }
}