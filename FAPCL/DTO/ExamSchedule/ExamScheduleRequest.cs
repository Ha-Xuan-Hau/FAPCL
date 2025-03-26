using System.ComponentModel.DataAnnotations;

namespace FAPCL.DTO.ExamSchedule
{
    public class ExamScheduleRequest
    {
        [Required]
        public string ExamName { get; set; }
        
        [Required]
        [MinLength(1)]
        [MaxLength(4)]
        public List<int> CourseIds { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
    }
}