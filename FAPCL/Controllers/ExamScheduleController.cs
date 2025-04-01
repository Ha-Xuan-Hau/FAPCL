using FAPCL.DTO.ExamSchedule;
using FAPCL.Services.examSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
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
        // [Authorize(Roles ="Admin")]
        public async Task<ActionResult<SchedulingResult>> ScheduleExams(ExamScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Validate date range
                TimeSpan dateRange = request.EndDate.Date - request.StartDate.Date;
                if (dateRange.Days < 0)
                    return BadRequest(new SchedulingResult
                    {
                        Success = false,
                        Message = "End date must be after start date"
                    });

                if (dateRange.Days > 13) // 14 days including both start and end
                    return BadRequest(new SchedulingResult
                    {
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
        // [Authorize(Roles = "Admin,Teacher")]
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

        [HttpGet("list")]
        // [AllowAnonymous]
        public async Task<IActionResult> GetExams([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _schedulingService.ListExamsAsync(startDate, endDate);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.ScheduledExams);
        }

        [HttpGet("currentSemeterCourses")]
        // [AllowAnonymous]
        public async Task<ActionResult<List<CourseDTO>>> GetCourses()
        {
            try
            {
                DateTime end = DateTime.Today;
                DateTime start = end.AddMonths(-3);
                var courses = await _schedulingService.GetCoursesAsync(start, end);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpGet("student/{studentId}")]
        // [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentExamSchedule(string studentId)
        {
            DateTime start = GetQuarterStartDate(DateTime.Today);
            DateTime end = GetQuarterEndDate(DateTime.Today);

            var result = await _schedulingService.GetStudentExamScheduleAsync(studentId, start, end);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpGet("teacher/{teacherId}")]
        // [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTeacherExamSchedule(string teacherId)
        {
            DateTime start = GetQuarterStartDate(DateTime.Today);
            DateTime end = GetQuarterEndDate(DateTime.Today);

            var result = await _schedulingService.GetTeacherExamScheduleAsync(teacherId, start, end);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }


        private static DateTime GetQuarterStartDate(DateTime date)
        {
            int quarterStartMonth = ((date.Month - 1) / 4) * 4 + 1;
            return new DateTime(date.Year, quarterStartMonth, 10);
        }

        private static DateTime GetQuarterEndDate(DateTime date)
        {
            int endMonth = ((date.Month - 1) / 4) * 4 + 4;
            int daysInMonth = DateTime.DaysInMonth(date.Year, endMonth);
            return new DateTime(date.Year, endMonth, daysInMonth);
        }

    }
}
