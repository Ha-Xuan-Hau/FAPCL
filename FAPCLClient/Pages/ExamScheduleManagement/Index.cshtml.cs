using FAPCL.DTO.ExamSchedule;
using FAPCLClient.Model;
using FAPCLClient.Model.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAPCLClient.Pages.ExamScheduleManagement
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexModel> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public List<ExamListItem> Exams { get; set; } = new List<ExamListItem>();
        public string ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(+14);

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        private const int PageSize = 10;

        public IndexModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var exams = await GetAllExamsAsync();
                if (exams != null)
                {
                    // Sort the full list first.
                    exams = exams.OrderByDescending(e => e.ExamDate)
                                 .ThenBy(e => e.StartTime)
                                 .ToList();

                    int totalExamCount = exams.Count;
                    TotalPages = (int)Math.Ceiling((double)totalExamCount / PageSize);
                    if (CurrentPage < 1)
                        CurrentPage = 1;
                    if (CurrentPage > TotalPages)
                        CurrentPage = TotalPages;

                    // Now paginate on the already sorted list.
                    Exams = exams
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exams");
                ErrorMessage = "Failed to load exam data. Please try again.";
                return Page();
            }
        }

        #region API Calls

        private async Task<List<ExamListItem>> GetAllExamsAsync()
        {
            try
            {
                var client = CreateHttpClient();
                // Pass startDate and endDate as query parameters if your API supports that.
                var response = await client.GetAsync($"ExamSchedule/list?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ExamListItem>>(content, _jsonOptions);
                }

                ErrorMessage = $"Failed to load exams: {response.ReasonPhrase}";
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exams");
                ErrorMessage = "Failed to load exams. Please try again.";
                return null;
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new Exception("ApiSettings:BaseUrl is not configured.");
            }
            client.BaseAddress = new Uri(baseUrl);

            var token = User.FindFirst("token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }
        #endregion
    }
}
