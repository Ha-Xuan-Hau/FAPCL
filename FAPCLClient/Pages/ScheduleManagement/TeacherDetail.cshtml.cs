using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FAPCL.DTO;
using FAPCL.DTO.FAPCL.DTO;

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
            var response = await _httpClient.GetAsync($"https://localhost:7007/api/teachers/{Id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            Teacher = await response.Content.ReadFromJsonAsync<TeacherDto>();
            return Page();
        }
    }
}
