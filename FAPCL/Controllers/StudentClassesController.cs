using FAPCL.DTO;
using FAPCL.DTO.ExamSchedule;
using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Mvc;

namespace FAPCL.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class StudentClassesController : ControllerBase
    {
        private readonly BookClassRoomContext _context;

        public StudentClassesController(BookClassRoomContext context)
        {
            _context = context;
        }

        // POST: api/StudentClasses
        [HttpPost]
        public async Task<IActionResult> EnrollStudents([FromBody] List<StudentClassCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest("Payload cannot be empty.");
            }

            foreach (var dto in dtos)
            {
                var enrollment = new StudentClass
                {
                    StudentId = dto.StudentId,
                    ClassId = dto.ClassId,
                    EnrollmentDate = dto.EnrollmentDate,
                    Status = string.IsNullOrEmpty(dto.Status) ? "Enrolled" : dto.Status
                };

                _context.StudentClasses.Add(enrollment);
            }
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Students enrolled successfully" });
        }

    }
}