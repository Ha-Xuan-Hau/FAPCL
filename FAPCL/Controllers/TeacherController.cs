using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using FAPCL.Model;
using FAPCL.DTO;

namespace FAPCL.Controllers
{
    [Route("api/teachers")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public TeacherController(BookClassRoomContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTeachers([FromQuery] string roleName = "Teacher")
        {
            var role = await _context.AspNetRoles
                .Where(r => r.Name == roleName)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound(new { message = "Role không tồn tại" });
            }

            var teachers = await _context.AspNetUsers
                .Join(_context.AspNetUserRoles,
                      user => user.Id,
                      userRole => userRole.UserId,
                      (user, userRole) => new { user, userRole })
                .Where(ur => ur.userRole.RoleId == role.Id) 
                .Select(t => new TeacherDto
                {
                    Id = t.user.Id,
                    UserName = t.user.UserName,
                    Email = t.user.Email,
                    PhoneNumber = t.user.PhoneNumber
                })
                .ToListAsync();

            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherById(string id)
        {
            var teacherRole = await _context.AspNetRoles
                .Where(r => r.Name == "Teacher")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (teacherRole == null)
            {
                return NotFound(new { message = "Role Teacher không tồn tại" });
            }

            var teacher = await _context.AspNetUsers
                .Join(_context.AspNetUserRoles,
                      user => user.Id,
                      userRole => userRole.UserId,
                      (user, userRole) => new { user, userRole })
                .Where(ur => ur.user.Id == id && ur.userRole.RoleId == teacherRole) 
                .Select(t => new TeacherDto
                {
                    Id = t.user.Id,
                    UserName = t.user.UserName,
                    Email = t.user.Email,
                    PhoneNumber = t.user.PhoneNumber
                })
                .FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound(new { message = "Giáo viên không tồn tại hoặc không có quyền Teacher" });
            }

            return Ok(teacher);
        }
    }
}
