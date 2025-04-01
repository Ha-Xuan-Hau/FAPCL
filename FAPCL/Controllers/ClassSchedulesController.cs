using FAPCL.DTO;
using FAPCL.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAPCL.Controllers
{
    [Route("api/class-schedules")]
    [ApiController]
    public class ClassSchedulesController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public ClassSchedulesController(BookClassRoomContext context)
        {
            _context = context;
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetSchedulesByClass(int classId)
        {
            if (classId == 0)
            {
                return BadRequest("Missing class ID.");
            }

            var schedules = await _context.ClassSchedules
                .Where(cs => cs.ClassId == classId)
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateSchedules([FromQuery] int classId, [FromBody] List<ClassScheduleDto> schedules)
        {
            if (classId <= 0 || schedules == null || !schedules.Any())
            {
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ!" });
            }

            var teacherId = await _context.Classes
                .Where(c => c.ClassId == classId)
                .Select(c => c.TeacherId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(teacherId))
            {
                return BadRequest(new { Message = "Không tìm thấy giáo viên cho lớp này!" });
            }

            var conflicts = await GetScheduleConflicts(teacherId, schedules);

            if (conflicts.Any())
            {
                return Conflict(new { Message = "Lịch dạy bị trùng!", Conflicts = conflicts });
            }

            var existingSchedules = await _context.ClassSchedules
                .Where(cs => cs.ClassId == classId)
                .ToListAsync();

            var schedulesToAdd = schedules
                .Where(s => !existingSchedules.Any(e => e.DayOfWeek == s.DayOfWeek && e.SlotId == s.SlotId))
                .Select(s => new ClassSchedule
                {
                    ClassId = classId,
                    SlotId = s.SlotId,
                    DayOfWeek = s.DayOfWeek
                })
                .ToList();

            var schedulesToRemove = existingSchedules
                .Where(e => !schedules.Any(s => s.DayOfWeek == e.DayOfWeek && s.SlotId == e.SlotId))
                .ToList();

            _context.ClassSchedules.RemoveRange(schedulesToRemove);
            _context.ClassSchedules.AddRange(schedulesToAdd);
            await _context.SaveChangesAsync();
            await UpdateStudentTimetable(classId);
            return Ok(new { Message = "Cập nhật lịch dạy thành công!" });
        }

        private async Task<List<ScheduleConflictDto>> GetScheduleConflicts(string teacherId, List<ClassScheduleDto> newSchedules)
        {
            var teacherClasses = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .Select(c => new
                {
                    c.ClassId,
                    c.StartDate,
                    c.EndDate
                })
                .ToListAsync();

            var existingSchedules = await _context.ClassSchedules
                .Where(cs => _context.Classes.Any(c =>
                    c.ClassId == cs.ClassId &&
                    c.TeacherId == teacherId))
                .Select(cs => new
                {
                    cs.ClassId,
                    cs.DayOfWeek,
                    cs.SlotId
                })
                .ToListAsync();

            var conflicts = newSchedules
                .Where(ns => existingSchedules.Any(es =>
                    es.DayOfWeek == ns.DayOfWeek &&
                    es.SlotId == ns.SlotId &&
                    es.ClassId != ns.ClassId)) 
                .Select(conflict => new ScheduleConflictDto
                {
                    DayOfWeek = conflict.DayOfWeek,
                    SlotId = conflict.SlotId,
                    ClassId = existingSchedules
                        .Where(es => es.DayOfWeek == conflict.DayOfWeek && es.SlotId == conflict.SlotId && es.ClassId != conflict.ClassId)
                        .Select(es => es.ClassId)
                        .FirstOrDefault()  
                })
                .ToList();

            return conflicts;
        }
        private async Task UpdateStudentTimetable(int classId)
        {
            var studentsInClass = await _context.StudentClasses
                .Where(sc => sc.ClassId == classId && sc.Status == "Enrolled")
                .Select(sc => sc.StudentId)
                .ToListAsync();

            var oldTimetable = _context.Timetables
                .Where(t => t.ClassId == classId && studentsInClass.Contains(t.StudentId));

            _context.Timetables.RemoveRange(oldTimetable);
            await _context.SaveChangesAsync();

            var updatedSchedules = await _context.ClassSchedules
                .Where(cs => cs.ClassId == classId)
                .ToListAsync();

            var newTimetables = new List<Timetable>();

            foreach (var studentId in studentsInClass)
            {
                foreach (var schedule in updatedSchedules)
                {
                    newTimetables.Add(new Timetable
                    {
                        StudentId = studentId,
                        ClassId = classId,
                        SlotId = schedule.SlotId,
                        DayOfWeek = schedule.DayOfWeek
                    });
                }
            }

            _context.Timetables.AddRange(newTimetables);
            await _context.SaveChangesAsync();
        }

    }
}
