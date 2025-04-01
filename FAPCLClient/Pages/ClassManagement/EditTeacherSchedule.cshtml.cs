using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FAPCL.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FAPCL.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FAPCLClient.Pages.ClassManagement
{
    public class EditTeacherScheduleModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api/";

        public EditTeacherScheduleModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }

        [BindProperty]
        public List<ClassSchedule> TeacherSchedules { get; set; } = new();

        [BindProperty]
        public List<Slot> AvailableSlots { get; set; } = new();

        [BindProperty]
        public List<string> DaysOfWeek { get; set; } = new()
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };
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
            if (role != "Admin")
            {
                return RedirectToPage("/Index");
            }
            if (ClassId == 0)
            {
                return BadRequest("Missing class ID.");
            }

            TeacherSchedules = await _httpClient.GetFromJsonAsync<List<ClassSchedule>>($"{ApiBaseUrl}class-schedules/class/{ClassId}");
            AvailableSlots = await _httpClient.GetFromJsonAsync<List<Slot>>($"{ApiBaseUrl}Slot");

            return Page();
        }

        public async Task<IActionResult> OnPostSaveScheduleAsync(List<string> SelectedSchedules)
        {
            var role = GetInfoFromToken().Role;
            if (role != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền.";
                return RedirectToPage("/Index");
            }
            if (ClassId == 0)
            {
                return BadRequest("Missing class ID.");
            }

            var schedules = new List<ClassScheduleDto>();

            if (SelectedSchedules != null)
            {
                foreach (var item in SelectedSchedules)
                {
                    var parts = item.Split('-');
                    if (parts.Length != 2 || !int.TryParse(parts[1], out int slotId))
                    {
                        TempData["Error"] = "Invalid schedule format.";
                        return RedirectToPage(new { ClassId });
                    }

                    schedules.Add(new ClassScheduleDto
                    {
                        ClassId = ClassId,
                        DayOfWeek = parts[0],
                        SlotId = slotId
                    });
                }
            }

            var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}class-schedules/update?classId={ClassId}", schedules);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var errorMessages = new List<string>();
                var errorDetails = await response.Content.ReadFromJsonAsync<JsonElement?>();

                if (errorDetails.HasValue)
                {
                    var message = errorDetails.Value.GetProperty("message").GetString();
                    errorMessages.Add(message);

                    if (errorDetails.Value.TryGetProperty("conflicts", out JsonElement conflicts) && conflicts.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var conflict in conflicts.EnumerateArray())
                        {
                            var classId = conflict.GetProperty("classId").GetInt32();
                            var dayOfWeek = conflict.GetProperty("dayOfWeek").GetString();
                            var slotId = conflict.GetProperty("slotId").GetInt32();

                            errorMessages.Add($"⚠ Trùng lịch: Lớp {classId}, {dayOfWeek} - Slot {slotId}");
                        }
                    }
                }

                TempData["Errors"] = JsonSerializer.Serialize(errorMessages);
                return RedirectToPage(new { ClassId });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                using var jsonDoc = JsonDocument.Parse(errorContent);
                string errorMessage = jsonDoc.RootElement.GetProperty("message").GetString();

                TempData["Error"] = errorMessage;
                return RedirectToPage(new { ClassId });
            }

            return RedirectToPage(new { ClassId });
        }
    }
}
