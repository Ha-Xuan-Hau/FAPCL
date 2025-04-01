using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FAPCLClient.Pages.New
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api/News";

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IList<News> News { get; set; } = new List<News>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Title { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync(int currentPage = 1)
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "title", Title },
                { "startDate", StartDate?.ToString("yyyy-MM-dd") ?? "" },
                { "endDate", EndDate?.ToString("yyyy-MM-dd") ?? "" },
                { "currentPage", currentPage.ToString() }
            };

            string queryString = string.Join("&", queryParams.Where(q => q.Value != null).Select(q => $"{q.Key}={q.Value}"));
            string url = $"{ApiBaseUrl}?{queryString}";

            var response = await _httpClient.GetFromJsonAsync<NewsResponse>(url);
            if (response != null)
            {
                News = response.News;
                TotalPages = response.TotalPages;
                CurrentPage = currentPage;
            }
        }
        public class NewsResponse
        {
            public List<News> News { get; set; } = new List<News>();
            public int TotalPages { get; set; }
        }
    }
}
