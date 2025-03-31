using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FAPCLClient.Pages.StudentEnroll
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<ClassEnrollmentDto> Classes { get; set; } = new List<ClassEnrollmentDto>();
        public List<ClassEnrollmentDto> RegisteredClasses { get; set; } = new List<ClassEnrollmentDto>();
        public string? Message { get; set; }

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Phương thức lấy studentId từ token
        private string GetStudentIdFromToken()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                Message = "Token không hợp lệ!";
                RedirectToPage("/Account/Login");
                return string.Empty;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var studentId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                Message = "Không tìm thấy studentId trong token!";
                RedirectToPage("/Account/Login");
                return string.Empty;
            }

            return studentId;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = GetStudentIdFromToken();
            if (string.IsNullOrEmpty(studentId)) return RedirectToPage("/Account/Login");

            var availableClassesResponse = await _httpClient.GetFromJsonAsync<List<ClassEnrollmentDto>>("http://localhost:5043/api/enroll/available-classes");

            if (availableClassesResponse != null)
            {
                Classes = availableClassesResponse;
            }

            var myClassesResponse = await _httpClient.GetFromJsonAsync<List<ClassEnrollmentDto>>($"http://localhost:5043/api/enroll/my-classes/{studentId}");

            if (myClassesResponse != null)
            {
                RegisteredClasses = myClassesResponse;
            }

            return Page();
        }

        // Phương thức đăng ký lớp học
        public async Task<IActionResult> OnPostRegisterAsync(int classId)
        {
            var studentId = GetStudentIdFromToken();
            if (string.IsNullOrEmpty(studentId)) return RedirectToPage("/Account/Login");

            var studentClass = new
            {
                StudentId = studentId,
                ClassId = classId,
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:5043/api/enroll/register", studentClass);

            if (response.IsSuccessStatusCode)
            {
                Message = "Đăng ký lớp học thành công!";
            }
            else
            {
                Message = await response.Content.ReadAsStringAsync();
            }

            return await OnGetAsync();
        }

        // Phương thức hủy đăng ký lớp học
        public async Task<IActionResult> OnPostCancelAsync(int classId)
        {
            var studentId = GetStudentIdFromToken();
            if (string.IsNullOrEmpty(studentId)) return RedirectToPage("/Account/Login");

            var studentClass = new
            {
                StudentId = studentId,
                ClassId = classId,
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:5043/api/enroll/cancel", studentClass);

            if (response.IsSuccessStatusCode)
            {
                Message = "Hủy đăng ký lớp học thành công!";
            }
            else
            {
                Message = await response.Content.ReadAsStringAsync();
            }

            return await OnGetAsync();
        }
    }
}
