using FAPCL.DTO;
using FAPCL.DTO.ExamSchedule;
using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
   [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public ClassesController(BookClassRoomContext context)
        {
            _context = context;
        }

        // POST: api/Classes
        [HttpPost]
        public async Task<IActionResult> CreateClasses([FromBody] List<ClassCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest("Payload cannot be empty.");
            }

            foreach (var dto in dtos)
            {
                var newClass = new Class
                {
                    ClassName = dto.ClassName,
                    CourseId = dto.CourseId,
                    TeacherId = dto.TeacherId,
                    RoomId = dto.RoomId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                };

                _context.Classes.Add(newClass);
            }
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Classes created successfully" });
        }

    }

}