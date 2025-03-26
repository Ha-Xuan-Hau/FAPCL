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
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.AspNetUsers
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    UserName = t.UserName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber
                })
                .ToListAsync();

            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherById(string id)
        {
            var teacher = await _context.AspNetUsers
                .Where(t => t.Id == id)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    UserName = t.UserName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber
                })
                .FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound(new { message = "Giáo viên không tồn tại" });
            }

            return Ok(teacher);
        }
    }
}
