using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Common;

namespace FAPCLClient.Pages.NewsManagement
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api/News";

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public string? Token { get; set; }

        public IList<News> News { get; set; } = new List<News>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Title { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public async Task<IActionResult> OnGetAsync(int currentPage = 1)
        {
            Token = HttpContext.Session.GetString("Token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            if (string.IsNullOrEmpty(Token))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var queryParams = new Dictionary<string, string?>
            {
                { "title", Title },
                { "startDate", StartDate?.ToString("yyyy-MM-dd") ?? "" },
                { "endDate", EndDate?.ToString("yyyy-MM-dd") ?? "" },
                { "currentPage", currentPage.ToString() }
            };

            string queryString = string.Join("&", queryParams.Where(q => q.Value != null).Select(q => $"{q.Key}={q.Value}"));
            string url = $"{ApiBaseUrl}/managerNews?{queryString}";

            var response = await _httpClient.GetFromJsonAsync<NewsResponse>(url);
            if (response != null)
            {
                News = response.News;
                TotalPages = response.TotalPages;
                CurrentPage = currentPage;
            }
            return Page();
        }
        [HttpPost]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            string url = $"{ApiBaseUrl}/{id}";

            var response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Không th? xóa tin t?c.");
                return Page();
            }

            // C?p nh?t danh sách tin t?c sau khi xóa
            return RedirectToPage();
        }
        public class NewsResponse
        {
            public List<News> News { get; set; } = new List<News>();
            public int TotalPages { get; set; }
        }
    }
}
