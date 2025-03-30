using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using FAPCL.DTO;
using FAPCL.DTO.FAPCL.DTO;
using System.Globalization;

namespace FAPCLClient.Pages.ClassManagement
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api/";

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ClassDto> Classes { get; set; } = new();
        public List<CourseDto> Courses { get; set; } = new();
        public List<TeacherDto> Teachers { get; set; } = new();
        public List<RoomDto> Rooms { get; set; } = new();
        [BindProperty] public string? ClassName { get; set; }
        [BindProperty] public int? SelectedCourseId { get; set; }

        public async Task OnGetAsync([FromQuery] string? className, [FromQuery] int? courseId)
        {
            ClassName = className;
            SelectedCourseId = courseId;

            Courses = await _httpClient.GetFromJsonAsync<List<CourseDto>>(ApiBaseUrl + "courses") ?? new();

            Teachers = await _httpClient.GetFromJsonAsync<List<TeacherDto>>(ApiBaseUrl + "teachers") ?? new();

            Rooms = await _httpClient.GetFromJsonAsync<List<RoomDto>>(ApiBaseUrl + "Room/admin/room") ?? new();

            var query = $"classes?className={className}&courseId={courseId}";
            Classes = await _httpClient.GetFromJsonAsync<List<ClassDto>>(ApiBaseUrl + query) ?? new();
        }

        public async Task<IActionResult> OnPostAsync([FromForm] string NewClassName,
                                                     [FromForm] string NewStartDate,
                                                     [FromForm] string NewEndDate,
                                                     [FromForm] int NewCourseId,
                                                     [FromForm] string NewTeacherId,
                                                     [FromForm] int NewRoomId)
        {
            if (string.IsNullOrWhiteSpace(NewClassName) || string.IsNullOrWhiteSpace(NewTeacherId))
            {
                TempData["ErrorMessage"] = "Tên lớp học và mã giáo viên không được để trống.";
                return Page();
            }

            if (!DateTime.TryParseExact(NewStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParseExact(NewEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                TempData["ErrorMessage"] = "Ngày bắt đầu hoặc ngày kết thúc không hợp lệ.";
                return Page();
            }

            if (startDate > endDate)
            {
                TempData["ErrorMessage"] = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
                return Page();
            }

            var newClass = new ClassDto
            {
                ClassName = NewClassName,
                StartDate = startDate,
                EndDate = endDate,
                CourseId = NewCourseId,
                TeacherId = NewTeacherId,
                RoomId = NewRoomId,
                CourseName = "",
                TeacherName = "",
                RoomName = ""
            };

            var response = await _httpClient.PostAsJsonAsync(ApiBaseUrl + "classes", newClass);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Lớp học tạo thành công";
                return RedirectToPage();
            }
            else
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {responseContent}";
                return Page();
            }
        }


    }
}
