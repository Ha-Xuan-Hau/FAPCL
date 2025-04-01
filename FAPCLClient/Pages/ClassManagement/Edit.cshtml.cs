using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FAPCL.DTO;
using FAPCL.DTO.FAPCL.DTO;

public class EditModel : PageModel
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "http://localhost:5043/api/";

    public EditModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [BindProperty] public ClassDto ClassDto { get; set; } = new();
    public List<CourseDto> Courses { get; set; } = new();
    public List<TeacherDto> Teachers { get; set; } = new();
    public List<RoomDto> Rooms { get; set; } = new();

    [BindProperty] public string? StartDate { get; set; }
    [BindProperty] public string? EndDate { get; set; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        ClassDto = await _httpClient.GetFromJsonAsync<ClassDto>($"{ApiBaseUrl}class-management/classes/{id}/dto") ?? new();
        StartDate = ClassDto.StartDate.ToString("yyyy-MM-dd");
        EndDate = ClassDto.EndDate.ToString("yyyy-MM-dd");

        var courseTask = _httpClient.GetFromJsonAsync<List<CourseDto>>($"{ApiBaseUrl}courses");
        var teacherTask = _httpClient.GetFromJsonAsync<List<TeacherDto>>($"{ApiBaseUrl}teachers");
        var roomTask = _httpClient.GetFromJsonAsync<List<RoomDto>>($"{ApiBaseUrl}Room/admin/room");

        await Task.WhenAll(courseTask, teacherTask, roomTask);

        Courses = courseTask.Result ?? new();
        Teachers = teacherTask.Result ?? new();
        Rooms = roomTask.Result ?? new();

        return Page();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(ClassDto.ClassName) || string.IsNullOrWhiteSpace(ClassDto.TeacherId))
        {
            TempData["ErrorMessage"] = "Tên lớp học và mã giáo viên không được để trống.";
            return Page();
        }

        if (!DateTime.TryParseExact(StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) ||
            !DateTime.TryParseExact(EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
        {
            TempData["ErrorMessage"] = "Ngày bắt đầu hoặc ngày kết thúc không hợp lệ.";
            return Page();
        }

        if (startDate > endDate)
        {
            TempData["ErrorMessage"] = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
            return Page();
        }

        ClassDto.StartDate = startDate;
        ClassDto.EndDate = endDate;
        ClassDto.RoomName = "";
        ClassDto.CourseName = "";
        ClassDto.TeacherName = "";

        var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}class-management/classes/{ClassDto.ClassId}", ClassDto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
            return RedirectToPage("./Index");
        }
        else
        {
            TempData["ErrorMessage"] = $"Có lỗi xảy ra: {responseContent}";
            return Page();
        }
    }
}
