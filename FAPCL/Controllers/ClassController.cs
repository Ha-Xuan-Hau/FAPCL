using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using FAPCL.DTO;
using FAPCL.Model;
using FAPCL.DTO.FAPCL.DTO;

namespace FAPCL.Controllers
{
    [Route("api/classes")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public ClassController(BookClassRoomContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
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

        [HttpGet("{id}/students")]
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

    }
}
