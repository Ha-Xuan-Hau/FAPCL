using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using FAPCL.DTO;
using FAPCL.Model;
using FAPCL.DTO.FAPCL.DTO;

namespace FAPCL.Controllers
{
    [Route("api/class-management")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public ClassController(BookClassRoomContext context)
        {
            _context = context;
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses([FromQuery] string? className, [FromQuery] int? courseId)
        {
            var query = _context.Classes
                        .AsNoTracking()
                        .Include(c => c.Teacher)
                        .Include(c => c.Course)
                        .Include(c => c.Room)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(className))
            {
                query = query.Where(c => c.ClassName.Contains(className));
            }
            if (courseId.HasValue)
            {
                query = query.Where(c => c.CourseId == courseId);
            }

            var classList = await query
                .Select(c => new ClassDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher.UserName ?? "N/A",
                    CourseId = c.CourseId,
                    CourseName = c.Course.CourseName ?? "N/A",
                    RoomId = c.RoomId,
                    RoomName = c.Room.RoomName ?? "N/A"
                })
                .ToListAsync();

            return Ok(classList);
        }

        [HttpPost("classes")]
        public async Task<IActionResult> AddClass([FromBody] ClassDto newClass)
        {
            if (newClass.StartDate > newClass.EndDate)
            {
                return BadRequest(new { message = "Ngày bắt đầu không được lớn hơn ngày kết thúc" });
            }

            var classEntity = new Class
            {
                ClassName = newClass.ClassName,
                StartDate = newClass.StartDate,
                EndDate = newClass.EndDate,
                TeacherId = newClass.TeacherId,
                CourseId = newClass.CourseId,
                RoomId = newClass.RoomId
            };

            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lớp học được thêm thành công", classId = classEntity.ClassId });
        }

        [HttpGet("classes/{id}")]
        public async Task<IActionResult> GetClassDetail(int id)
        {
            var classDetail = await _context.Classes
                .AsNoTracking()
                .Where(c => c.ClassId == id)
                .Select(c => new ClassDetailDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Course = new CourseDto
                    {
                        CourseId = c.Course.CourseId,
                        CourseName = c.Course.CourseName,
                        Description = c.Course.Description,
                        Credits = c.Course.Credits
                    },
                    Teacher = new TeacherDto
                    {
                        Id = c.Teacher.Id,
                        UserName = c.Teacher.UserName
                    }
                })
                .FirstOrDefaultAsync();

            if (classDetail == null)
            {
                return NotFound(new { message = "Lớp học không tồn tại" });
            }

            return Ok(classDetail);
        }

        [HttpGet("classes/{id}/students")]
        public async Task<IActionResult> GetClassStudents(int id)
        {
            var students = await _context.StudentClasses
                .Where(sc => sc.ClassId == id && sc.Status == "Enrolled")
                .Include(sc => sc.Student)
                .Select(sc => new StudentDto
                {
                    Id = sc.Student.Id,
                    UserName = sc.Student.UserName,
                    Email = sc.Student.Email,
                    PhoneNumber = sc.Student.PhoneNumber
                })
                .ToListAsync();

            if (!students.Any())
            {
                return NotFound(new { message = "Không có sinh viên nào đã đăng ký trong lớp này" });
            }

            return Ok(students);
        }

        [HttpPut("classes/{id}")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassDto updatedClass)
        {
            var classEntity = await _context.Classes.FindAsync(id);

            if (classEntity == null)
                return NotFound(new { message = "Lớp học không tồn tại" });

            if (updatedClass.StartDate > updatedClass.EndDate)
                return BadRequest(new { message = "Ngày bắt đầu không được lớn hơn ngày kết thúc" });

            classEntity.ClassName = updatedClass.ClassName;
            classEntity.StartDate = updatedClass.StartDate;
            classEntity.EndDate = updatedClass.EndDate;
            classEntity.TeacherId = updatedClass.TeacherId;
            classEntity.CourseId = updatedClass.CourseId;
            classEntity.RoomId = updatedClass.RoomId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Lớp học đã được cập nhật thành công" });
        }

        [HttpGet("classes/{id}/dto")]
        public async Task<IActionResult> GetClassDtoById(int id)
        {
            var classDto = await _context.Classes
                .AsNoTracking()
                .Where(c => c.ClassId == id)
                .Select(c => new ClassDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher.UserName ?? "N/A",
                    CourseId = c.CourseId,
                    CourseName = c.Course.CourseName ?? "N/A",
                    RoomId = c.RoomId,
                    RoomName = c.Room.RoomName ?? "N/A"
                })
                .FirstOrDefaultAsync();

            if (classDto == null)
            {
                return NotFound(new { message = "Lớp học không tồn tại" });
            }

            return Ok(classDto);
        }
    }
}
