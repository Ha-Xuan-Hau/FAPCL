using FAPCL.DTO;
using FAPCL.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FAPCL.Controllers
{
    [Route("api/schedule")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public ScheduleController(BookClassRoomContext context)
        {
            _context = context;
        }
        private string GetIdFromToken()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return string.Empty; 
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var studentId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            return studentId ?? string.Empty;
        }

        [HttpGet]
        public async Task<ActionResult> GetTimetable(
            [FromQuery] string fromDateMonth,
            [FromQuery] string toDateMonth,
            [FromQuery] string Year)
        {

          var studentId = GetIdFromToken(); 
            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized("Student is not logged in.");
            }

            // Xác thực và parse tham số năm và ngày
            if (!int.TryParse(Year, out int year))
            {
                return BadRequest("Invalid year format.");
            }

            // Định dạng ngày theo dd-MM-yyyy 
            if (!DateTime.TryParseExact($"{fromDateMonth}-{year}", "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact($"{toDateMonth}-{year}", "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                return BadRequest("Invalid date format. Use dd-MM-yyyy.");
            }

            var timetables = await _context.Timetables
                .Where(t => t.StudentId == studentId &&
                            t.Class.StartDate <= endDate &&
                            t.Class.EndDate >= startDate)
                .Select(t => new
                {
                    t.TimetableId,
                    t.DayOfWeek,
                    // Lấy khoảng thời gian lớp học từ bảng Classes
                    ClassStartDate = t.Class.StartDate,
                    ClassEndDate = t.Class.EndDate,
                    // Lấy thông tin Slot từ bảng Slots
                    SlotName = t.Slot.SlotName,
                    SlotStartTime = t.Slot.StartTime,
                    SlotEndTime = t.Slot.EndTime,
                    // Lấy các thông tin liên quan từ bảng Classes (Course, Teacher, Room)
                    ClassName = t.Class.ClassName,
                    CourseName = t.Class.Course.CourseName,
                    TeacherUserName = t.Class.Teacher.UserName,
                    RoomName = t.Class.Room.RoomName,
                    ClassId = t.Class.ClassId
                })
                .ToListAsync();

            var scheduleEntries = new List<ScheduleEntryDto>();

            foreach (var timetable in timetables)
            {
                // Xác định phạm vi thời gian hiệu quả của lớp so với tham số truyền vào
                var effectiveStart = timetable.ClassStartDate > startDate ? timetable.ClassStartDate : startDate;
                var effectiveEnd = timetable.ClassEndDate < endDate ? timetable.ClassEndDate : endDate;

                if (Enum.TryParse(timetable.DayOfWeek, true, out DayOfWeek dayOfWeek))
                {
                    foreach (var date in GetDatesInRange(effectiveStart, effectiveEnd, dayOfWeek))
                    {
                        var entry = new ScheduleEntryDto
                        {
                            Date = date,
                            SlotName = timetable.SlotName,
                            StartTime = timetable.SlotStartTime,
                            EndTime = timetable.SlotEndTime,
                            ClassName = timetable.ClassName,
                            CourseName = timetable.CourseName,
                            TeacherName = timetable.TeacherUserName,
                            RoomName = timetable.RoomName,
                            DayOfWeek = timetable.DayOfWeek,
                            ClassId = timetable.ClassId
                        };
                        scheduleEntries.Add(entry);
                    }
                }
            }

            return Ok(scheduleEntries);
        }

        private static IEnumerable<DateTime> GetDatesInRange(DateTime start, DateTime end, DayOfWeek dayOfWeek)
        {
            int daysUntilFirst = ((int)dayOfWeek - (int)start.DayOfWeek + 7) % 7;
            DateTime firstDate = start.AddDays(daysUntilFirst);
            if (firstDate > end)
            {
                yield break;
            }
            DateTime current = firstDate;
            while (current <= end)
            {
                yield return current;
                current = current.AddDays(7);
            }
        }


        [HttpGet("teacher")]
        public async Task<ActionResult> GetTeacherSchedule(
    [FromQuery] string fromDateMonth,
    [FromQuery] string toDateMonth,
    [FromQuery] string Year)
        {
           var teacherId = GetIdFromToken(); 
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("Teacher is not logged in.");
            }

            if (!int.TryParse(Year, out int year))
            {
                return BadRequest("Invalid year format.");
            }

            if (!DateTime.TryParseExact($"{fromDateMonth}-{year}", "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact($"{toDateMonth}-{year}", "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                return BadRequest("Invalid date format. Use dd-MM-yyyy.");
            }

            var schedules = await _context.ClassSchedules
                .Where(cs => cs.Class.TeacherId == teacherId)
                .Select(cs => new
                {
                    cs.ClassScheduleId,
                    cs.ClassId,
                    cs.DayOfWeek,
                    SlotName = cs.Slot.SlotName,
                    SlotStartTime = cs.Slot.StartTime,
                    SlotEndTime = cs.Slot.EndTime,
                    ClassName = cs.Class.ClassName,
                    CourseName = cs.Class.Course.CourseName,
                    RoomName = cs.Class.Room.RoomName,
                    ClassStartDate = cs.Class.StartDate,
                    ClassEndDate = cs.Class.EndDate
                })
                .ToListAsync();

            var scheduleEntries = new List<ScheduleEntryDto>();

            foreach (var schedule in schedules)
            {
                var effectiveStart = schedule.ClassStartDate > startDate ? schedule.ClassStartDate : startDate;
                var effectiveEnd = schedule.ClassEndDate < endDate ? schedule.ClassEndDate : endDate;

                if (Enum.TryParse(schedule.DayOfWeek, true, out DayOfWeek dayOfWeek))
                {
                    foreach (var date in GetDatesInRange(effectiveStart, effectiveEnd, dayOfWeek))
                    {
                        scheduleEntries.Add(new ScheduleEntryDto
                        {
                            Date = date,
                            SlotName = schedule.SlotName,
                            StartTime = schedule.SlotStartTime,
                            EndTime = schedule.SlotEndTime,
                            ClassName = schedule.ClassName,
                            CourseName = schedule.CourseName,
                            RoomName = schedule.RoomName,
                            DayOfWeek = schedule.DayOfWeek,
                            ClassId = schedule.ClassId
                        });
                    }
                }
            }

            return Ok(scheduleEntries);
        }

    }
}
