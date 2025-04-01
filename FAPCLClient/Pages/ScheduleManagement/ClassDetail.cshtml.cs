using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FAPCL.DTO;
using FAPCL.DTO.FAPCL.DTO;

namespace FAPCLClient.Pages.ScheduleManagement
{
    public class ClassDetailModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ClassDetailModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }
        public ClassDetailDto ClassDetail { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _httpClient.GetAsync($"http://localhost:5043/api/class-management/classes/{ClassId}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            ClassDetail = await response.Content.ReadFromJsonAsync<ClassDetailDto>();
            return Page();
        }
    }
}
