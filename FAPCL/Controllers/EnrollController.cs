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
            var classes = _context.StudentClasses
                .Where(sc => sc.StudentId == studentId && sc.Status == "Enrolled")
                .Select(sc => new ClassEnrollmentDto
                {
                    ClassId = sc.ClassId,
                    ClassName = sc.Class.ClassName,
                    StartDate = sc.Class.StartDate,
                    EndDate = sc.Class.EndDate,
                    RoomId = sc.Class.RoomId,
                    RoomName = sc.Class.Room.RoomName,
                    Capacity = sc.Class.Room.Capacity,
                    RegisteredCount = sc.Class.StudentClasses.Count(s => s.Status == "Enrolled")
                })
                .ToList();

            return Ok(classes);
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
                return BadRequest("Thông tin đăng ký không hợp lệ.");
            }

            var classInfo = _context.Classes
                .Include(c => c.Room) 
                .FirstOrDefault(c => c.ClassId == studentClassDto.ClassId);

            if (classInfo == null)
            {
                return NotFound("Lớp học không tồn tại.");
            }

            int currentEnrollmentCount = _context.StudentClasses.Count(sc => sc.ClassId == studentClassDto.ClassId && sc.Status == "Enrolled");

            if (currentEnrollmentCount >= classInfo.Room.Capacity)
            {
                return BadRequest("Lớp học đã đầy. Vui lòng chọn lớp học khác.");
            }

            var existingEnrollment = _context.StudentClasses
                .FirstOrDefault(sc => sc.StudentId == studentClassDto.StudentId && sc.ClassId == studentClassDto.ClassId);

            if (existingEnrollment != null)
            {
                return BadRequest("Bạn đã đăng ký lớp học này rồi.");
            }

            var studentClass = new StudentClass
            {
                StudentId = studentClassDto.StudentId,
                ClassId = studentClassDto.ClassId,
                Status = "Enrolled",
                EnrollmentDate = DateTime.Now
            };

            _context.StudentClasses.Add(studentClass);
            _context.SaveChanges();

            return Ok("Đăng ký lớp học thành công. Vui lòng chờ admin duyệt.");
        }

        [HttpPost("cancel")]
        public IActionResult CancelRegistration([FromBody] StudentClassRegisterDto studentClassDto)
        {
            if (studentClassDto == null || string.IsNullOrEmpty(studentClassDto.StudentId) || studentClassDto.ClassId <= 0)
            {
                return BadRequest("Thông tin hủy đăng ký không hợp lệ.");
            }

            var existingEnrollment = _context.StudentClasses
                .FirstOrDefault(sc => sc.StudentId == studentClassDto.StudentId && sc.ClassId == studentClassDto.ClassId);

            if (existingEnrollment == null)
            {
                return NotFound("Không tìm thấy lớp học đăng ký của sinh viên.");
            }

            // Hủy đăng ký
            _context.StudentClasses.Remove(existingEnrollment);
            _context.SaveChanges();

            return Ok("Hủy đăng ký lớp học thành công.");
        }

    }
}
