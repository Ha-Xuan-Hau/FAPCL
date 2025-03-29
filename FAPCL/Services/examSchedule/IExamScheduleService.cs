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
        
    //    Task<List<CourseDTO>> GetCoursesAsync(DateTime startDate, DateTime endDate);
    }
}
