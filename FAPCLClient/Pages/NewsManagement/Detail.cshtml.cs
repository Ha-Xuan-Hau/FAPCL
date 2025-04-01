using FAPCL.Services;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using static FAPCLClient.Pages.NewsManagement.IndexModel;

namespace FAPCLClient.Pages.NewsManagement
{
    public class DetailModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api/News";
        public News News { get; set; }

        public DetailModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string url = $"{ApiBaseUrl}/managerNews/{id}";
            News = await _httpClient.GetFromJsonAsync<News>(url);
            if (News == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
