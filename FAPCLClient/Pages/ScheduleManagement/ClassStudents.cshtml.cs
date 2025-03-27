using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using FAPCL.DTO;

namespace FAPCLClient.Pages.ScheduleManagement
{
    public class ClassStudentsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ClassStudentsModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty(SupportsGet = true)]
        public int ClassId { get; set; }
        public List<StudentDto> Students { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _httpClient.GetAsync($"http://localhost:5043/api/classes/{ClassId}/students");
            if (!response.IsSuccessStatusCode)
            {
                Students = new List<StudentDto>(); 
                return Page();
            }

            Students = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
            return Page();
        }
    }
}
