using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FAPCL.DTO;
namespace FAPCLClient.Pages.ScheduleManagement
{
    public class TeacherDetailModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public TeacherDetailModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }
        public TeacherDto Teacher { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Identity/Account/Login");
            }
            var response = await _httpClient.GetAsync($"http://localhost:5043/api/teachers/{Id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            Teacher = await response.Content.ReadFromJsonAsync<TeacherDto>();
            return Page();
        }
    }
}
