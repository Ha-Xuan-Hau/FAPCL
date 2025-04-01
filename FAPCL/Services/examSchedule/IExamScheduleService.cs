using FAPCL.DTO.ExamSchedule;

namespace FAPCL.Services.examSchedule
{
    public interface IExamScheduleService
    {
        Task<SchedulingResult> ScheduleExamsAsync(
            string examName,
            List<int> courseIds,
            DateTime startDate,
            DateTime endDate);
        Task<DetailedExamResult> GetScheduleDetailsAsync(int scheduleId);
        Task<SchedulingResult> ListExamsAsync(DateTime startDate, DateTime endDate);
        Task<List<CourseDTO>> GetCoursesAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<StudentExamScheduleDTO>>> GetStudentExamScheduleAsync(string studentId, DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<StudentExamScheduleDTO>>> GetTeacherExamScheduleAsync(string TeacherId, DateTime startDate, DateTime endDate);
    }
}
