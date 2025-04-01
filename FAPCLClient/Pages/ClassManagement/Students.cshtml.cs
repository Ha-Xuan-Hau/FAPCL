using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc;
using FAPCLClient.Model;

namespace FAPCLClient.Pages.ClassManagement
{
    public class StudentsModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api/";

        public StudentsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<StudentClassDto> Students { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        public async Task OnGetAsync()
        {
            if (ClassId == 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin lớp học.";
                return;
            }

            var response = await _httpClient.GetFromJsonAsync<ApiResponse>(ApiBaseUrl + $"enroll/class-students/{ClassId}");

            if (response != null)
            {
                if (!string.IsNullOrEmpty(response.Message))
                {
                    TempData["SuccessMessage"] = response.Message;
                }

                if (response.Students != null)
                {
                    Students = response.Students;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể lấy danh sách sinh viên.";
            }
        }

        public async Task<IActionResult> OnPostChangeMultipleStatusAsync(string[] selectedStudents, string newStatus)
        {
            var validStatuses = new[] { "Pending", "Enrolled", "Dropped", "Completed", "Canceled" };

            if (selectedStudents == null || selectedStudents.Length == 0)
            {
                TempData["ErrorMessage"] = "Bạn chưa chọn sinh viên nào.";
                return RedirectToPage(new { ClassId = this.ClassId });
            }

            if (!validStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                return RedirectToPage(new { ClassId = this.ClassId });
            }

            foreach (var studentId in selectedStudents)
            {
                var updateDto = new UpdateStudentStatusDto
                {
                    StudentId = studentId,
                    ClassId = this.ClassId, 
                    Status = newStatus
                };

                var response = await _httpClient.PutAsJsonAsync(ApiBaseUrl + "enroll/update-status", updateDto);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

                    if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Message))
                    {
                        TempData["SuccessMessage"] = apiResponse.Message;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Cập nhật trạng thái thất bại.";
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    TempData["ErrorMessage"] = errorResponse?.Message ?? "Cập nhật trạng thái thất bại.";
                    return RedirectToPage(new { ClassId = this.ClassId });
                }
            }

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
            return RedirectToPage(new { ClassId = this.ClassId });
        }


        public class ApiResponse
        {
            public string Message { get; set; }
            public List<StudentClassDto> Students { get; set; }
        }
    }
}
