using System.ComponentModel.DataAnnotations;

namespace FAPCL.DTO.ExamSchedule
{
public class ClassCreateDto
    {
        public string ClassName { get; set; } = null!;
        public int CourseId { get; set; }
        public string TeacherId { get; set; } = null!;
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}