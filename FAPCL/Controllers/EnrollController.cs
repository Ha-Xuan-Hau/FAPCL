using Microsoft.AspNetCore.Mvc;
using FAPCL.Model;
using System.Linq;
using FAPCL.DTO;
using Microsoft.EntityFrameworkCore;

namespace FAPCL.Controllers
{
    [Route("api/enroll")]
    [ApiController]
    public class EnrollController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public EnrollController(BookClassRoomContext context)
        {
            _context = context;
        }

        [HttpGet("my-classes/{studentId}")]
        public IActionResult GetMyClasses(string studentId)
        {
            var currentDate = DateTime.Now;

            var classes = _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .OrderBy(sc => sc.Class.StartDate)
                .Select(sc => new ClassEnrollmentDto
                {
                    ClassId = sc.ClassId,
                    ClassName = sc.Class.ClassName,
                    StartDate = sc.Class.StartDate,
                    EndDate = sc.Class.EndDate,
                    RoomId = sc.Class.RoomId,
                    RoomName = sc.Class.Room.RoomName,
                    Capacity = sc.Class.Room.Capacity,
                    RegisteredCount = sc.Class.StudentClasses.Count(s => s.Status == "Enrolled"),
                    Status = sc.Status
                })
                .ToList();
            return Ok( classes );
        }

        [HttpGet("available-classes")]
        public IActionResult GetAvailableClasses()
        {
            var classes = _context.Classes
                .Select(c => new ClassEnrollmentDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    RoomId = c.RoomId,
                    RoomName = c.Room.RoomName,
                    Capacity = c.Room.Capacity,
                    RegisteredCount = c.StudentClasses.Count(s => s.Status == "Enrolled")
                })
                .ToList();
            return Ok(classes);
        }


        [HttpPost("register")]
        public IActionResult RegisterStudent([FromBody] StudentClassRegisterDto studentClassDto)
        {
            if (studentClassDto == null || string.IsNullOrEmpty(studentClassDto.StudentId) || studentClassDto.ClassId <= 0)
            {
                return BadRequest(new { message = "Thông tin đăng ký không hợp lệ." });
            }

            int totalCredits = _context.StudentClasses
                .Where(sc => sc.StudentId == studentClassDto.StudentId && (sc.Status == "Enrolled" || sc.Status == "Pending"))
                .Join(_context.Classes, sc => sc.ClassId, c => c.ClassId, (sc, c) => c.CourseId)
                .Join(_context.Courses, cId => cId, course => course.CourseId, (cId, course) => course.Credits)
                .Sum();

            var classInfo = _context.Classes
                .Include(c => c.Room)
                .Include(c => c.Course)
                .FirstOrDefault(c => c.ClassId == studentClassDto.ClassId);

            if (classInfo == null) return NotFound(new { message = "Lớp học không tồn tại." });
            if (classInfo.EndDate < DateTime.Now) return BadRequest(new { message = "Lớp học này đã kết thúc. Bạn không thể đăng ký." });

            int classCredits = classInfo.Course.Credits;

            if (totalCredits + classCredits > 20)
            {
                return BadRequest(new { message = $"Bạn đã đăng ký {totalCredits} tín chỉ. Không thể đăng ký thêm {classCredits} tín chỉ vì vượt quá giới hạn 20." });
            }

            int currentEnrollmentCount = _context.StudentClasses
                .Count(sc => sc.ClassId == studentClassDto.ClassId && sc.Status == "Enrolled");

            if (currentEnrollmentCount >= classInfo.Room.Capacity)
                return BadRequest(new { message = "Lớp học đã đầy. Vui lòng chọn lớp khác." });

            var existingEnrollment = _context.StudentClasses
                .FirstOrDefault(sc => sc.StudentId == studentClassDto.StudentId && sc.ClassId == studentClassDto.ClassId);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.Status == "Enrolled")
                    return BadRequest(new { message = "Bạn đã đăng ký lớp này rồi." });
                if (existingEnrollment.Status == "Pending")
                    return BadRequest(new { message = "Yêu cầu đăng ký của bạn đang chờ duyệt." });
            }

            var studentClass = new StudentClass
            {
                StudentId = studentClassDto.StudentId,
                ClassId = studentClassDto.ClassId,
                Status = "Pending",
                EnrollmentDate = DateTime.Now
            };

            _context.StudentClasses.Add(studentClass);
            _context.SaveChanges();

            return Ok(new { message = "Đăng ký lớp học thành công, đang chờ duyệt." });
        }

        [HttpPost("cancel")]
        public IActionResult CancelRegistration([FromBody] StudentClassRegisterDto studentClassDto)
        {
            if (studentClassDto == null || string.IsNullOrEmpty(studentClassDto.StudentId) || studentClassDto.ClassId <= 0)
            {
                return BadRequest(new { message = "Thông tin đăng ký không hợp lệ." });
            }

            var existingEnrollment = _context.StudentClasses
                .FirstOrDefault(sc => sc.StudentId == studentClassDto.StudentId && sc.ClassId == studentClassDto.ClassId);

            if (existingEnrollment == null) return NotFound(new { message = "Không tìm thấy lớp học đăng ký của sinh viên." });
            if (existingEnrollment.Status != "Pending") return BadRequest(new { message = "Chỉ có thể hủy lớp đang chờ duyệt." });

            _context.StudentClasses.Remove(existingEnrollment);
            _context.SaveChanges();

            return Ok(new { message = "Đã xóa đăng ký lớp học thành công." });
        }

        [HttpGet("class-students/{classId}")]
        public IActionResult GetClassStudents(int classId)
        {
            var students = _context.StudentClasses
                .Where(sc => sc.ClassId == classId)
                .Include(sc => sc.Student)
                .Include(sc => sc.Class)
                .Select(sc => new StudentClassDto
                {
                    StudentId = sc.StudentId,
                    FullName = sc.Student.FirstName + " " + sc.Student.LastName,
                    Email = sc.Student.Email,
                    Status = sc.Status,
                    ClassName = sc.Class.ClassName,
                    StartDate = sc.Class.StartDate,
                    EndDate = sc.Class.EndDate
                }).ToList();

            return Ok(new { message = "Danh sách sinh viên trong lớp học.", students });
        }

        [HttpPut("update-status")]
        public IActionResult UpdateStudentStatus([FromBody] UpdateStudentStatusDto updateDto)
        {
            var enrollment = _context.StudentClasses
                .FirstOrDefault(sc => sc.StudentId == updateDto.StudentId && sc.ClassId == updateDto.ClassId);

            if (enrollment == null) return NotFound(new { message = "Không tìm thấy sinh viên trong lớp." });

            enrollment.Status = updateDto.Status;
            _context.SaveChanges();

            if (updateDto.Status == "Enrolled")
            {
                var classSchedules = _context.ClassSchedules
                    .Where(cs => cs.ClassId == updateDto.ClassId)
                    .ToList();

                if (!classSchedules.Any())
                {
                    return BadRequest(new { message = "Không có slot dạy của giáo viên trong lớp này." });
                }

                foreach (var schedule in classSchedules)
                {
                    var newTimetable = new Timetable
                    {
                        StudentId = updateDto.StudentId,
                        ClassId = updateDto.ClassId,
                        SlotId = schedule.SlotId,
                        DayOfWeek = schedule.DayOfWeek
                    };

                    _context.Timetables.Add(newTimetable);
                }

                _context.SaveChanges();
                return Ok(new { message = "Cập nhật trạng thái thành công và thêm vào thời khóa biểu." });
            }
            else if (updateDto.Status == "Canceled" || updateDto.Status == "Completed" || updateDto.Status == "Pending")
            {
                var timetableRecords = _context.Timetables
                    .Where(t => t.StudentId == updateDto.StudentId && t.ClassId == updateDto.ClassId)
                    .ToList();

                if (timetableRecords.Any())
                {
                    _context.Timetables.RemoveRange(timetableRecords);
                    _context.SaveChanges();
                }

                return Ok(new { message = "Cập nhật trạng thái thành công và đã xóa khỏi thời khóa biểu." });
            }

            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }


        [HttpPost("timetable/add")]
        public IActionResult AddTimetable([FromBody] Timetable timetable)
        {
            var existingTimetable = _context.Timetables
                .FirstOrDefault(t => t.StudentId == timetable.StudentId && t.ClassId == timetable.ClassId && t.SlotId == timetable.SlotId);

            if (existingTimetable != null)
            {
                return BadRequest(new { message = "Lịch học này đã tồn tại." });
            }

            _context.Timetables.Add(timetable);
            _context.SaveChanges();

            return Ok(new { message = "Thêm lịch học thành công." });
        }

        [HttpDelete("timetable/delete")]
        public IActionResult DeleteTimetable(string studentId, int classId)
        {
            var timetableRecords = _context.Timetables
                .Where(t => t.StudentId == studentId && t.ClassId == classId)
                .ToList();

            if (!timetableRecords.Any())
            {
                return NotFound(new { message = "Không tìm thấy lịch học của sinh viên." });
            }

            _context.Timetables.RemoveRange(timetableRecords);
            _context.SaveChanges();

            return Ok(new { message = "Xóa lịch học thành công." });
        }
    }
}
