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
                // Load all exams
                var exams = await GetAllExamsAsync();
                if (exams != null)
                {
                    Exams = exams;
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

        private async Task<List<ExamListItem>> GetAllExamsAsync()
        {
            try
            {
                var client = CreateHttpClient();
                var response = await client.GetAsync("api/examschedule/list");

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
            client.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);

            // Get the token from the user claims
            var token = User.FindFirst("token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }
    }

    // DTO to represent items in the exam list
    public class ExamListItem
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public DateTime ExamDate { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomName { get; set; }
        public int StudentCount { get; set; }
    }
}
