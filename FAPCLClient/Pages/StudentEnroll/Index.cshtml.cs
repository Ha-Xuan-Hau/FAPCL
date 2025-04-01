using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NuGet.Common;

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

        private (string Id, string Role) GetInfoFromToken()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return (string.Empty, string.Empty);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var Id = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            var role = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(Id))
            {
                RedirectToPage("/Account/Login");
                return (string.Empty, string.Empty);
            }

            return (Id, role);
        }


        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = GetInfoFromToken().Id;
            var role = GetInfoFromToken().Role;
            if (string.IsNullOrEmpty(studentId))
            {
                return Redirect("~/Identity/Account/Login");
            }
            if (role != "Student")
            {
                return RedirectToPage("/Index");
            }
            var availableClassesResponse = await _httpClient.GetFromJsonAsync<List<ClassEnrollmentDto>>("http://localhost:5043/api/enroll/available-classes");
            Classes = availableClassesResponse ?? new List<ClassEnrollmentDto>();

            var myClassesResponse = await _httpClient.GetFromJsonAsync<List<ClassEnrollmentDto>>($"http://localhost:5043/api/enroll/my-classes/{studentId}");
            RegisteredClasses = myClassesResponse ?? new List<ClassEnrollmentDto>();

            return Page();
        }


        public async Task<IActionResult> OnPostRegisterAsync(int classId)
        {
            var studentId = GetInfoFromToken().Id;
            var role = GetInfoFromToken().Role;
            if (string.IsNullOrEmpty(studentId))
            {
                return Redirect("~/Identity/Account/Login");
            }
            if (role != "Student")
            {
                return RedirectToPage("/Index");
            }

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

        public async Task<IActionResult> OnPostCancelAsync(int classId)
        {
            var studentId = GetInfoFromToken().Id;
            var role = GetInfoFromToken().Role;
            if (string.IsNullOrEmpty(studentId))
            {
                return Redirect("~/Identity/Account/Login");
            }
            if (role != "Student")
            {
                return RedirectToPage("/Index");
            }

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
