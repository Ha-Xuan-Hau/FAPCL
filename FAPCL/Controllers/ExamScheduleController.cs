using FAPCL.DTO.ExamSchedule;
using FAPCL.Services.examSchedule;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamScheduleController : ControllerBase
    {
        private readonly IExamScheduleService _schedulingService;
        private readonly ILogger<ExamScheduleController> _logger;
        
        public ExamScheduleController(
            IExamScheduleService schedulingService,
            ILogger<ExamScheduleController> logger)
        {
            _schedulingService = schedulingService;
            _logger = logger;
        }
        
        [HttpPost]
        public async Task<ActionResult<SchedulingResult>> ScheduleExams(ExamScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                // Validate date range
                TimeSpan dateRange = request.EndDate.Date - request.StartDate.Date;
                if (dateRange.Days < 0)
                    return BadRequest(new SchedulingResult { 
                        Success = false, 
                        Message = "End date must be after start date" 
                    });
                    
                if (dateRange.Days > 13) // 14 days including both start and end
                    return BadRequest(new SchedulingResult { 
                        Success = false, 
                        Message = "Exam period cannot exceed 14 days" 
                    });
                
                var result = await _schedulingService.ScheduleExamsAsync(
                    request.ExamName,
                    request.CourseIds,
                    request.StartDate,
                    request.EndDate);
                    
                if (result.Success)
                    return Ok(result);
                    
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling exams");
                return StatusCode(500, new SchedulingResult 
                { 
                    Success = false, 
                    Message = "An unexpected error occurred during scheduling"
                });
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedExamResult>> GetScheduleDetails(int id)
        {
            try
            {
                var result = await _schedulingService.GetScheduleDetailsAsync(id);
                
                if (result.Success)
                    return Ok(result);
                    
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule details");
                return StatusCode(500, new SchedulingResult 
                { 
                    Success = false, 
                    Message = "An unexpected error occurred"
                });
            }
        }
        
        // [HttpGet("courses")]
        // public async Task<ActionResult<List<CourseDTO>>> GetCourses(DateTime startDate, DateTime endDate)
        // {
        //     try
        //     {
        //         var courses = await _schedulingService.GetCoursesAsync(startDate, endDate);
        //         return Ok(courses);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error retrieving courses");
        //         return StatusCode(500, "An unexpected error occurred");
        //     }
        // }
    }
}
