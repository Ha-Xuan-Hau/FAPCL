using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using FAPCLClient.Model;
using FAPCLClient.Model.DTOs;

namespace FAPCLClient.Pages.ExamScheduleManagement  
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DetailsModel> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public List<ScheduledExamDTO> ScheduledExams { get; set; }
        public string ExamName { get; set; }
        public string ErrorMessage { get; set; }

        public DetailsModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<DetailsModel> logger)
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
                var result = await GetScheduleDetailsAsync(Id);

                if (result == null || !result.Success)
                {
                    ErrorMessage = result?.Message ?? "Failed to retrieve schedule details.";
                    return Page();
                }

                ScheduledExams = result.ScheduledExams;

                if (ScheduledExams != null && ScheduledExams.Any())
                {
                    // Extract the exam name from the first exam
                    var examNameParts = ScheduledExams.First().ExamName?.Split("[Session:");
                    ExamName = examNameParts?.Length > 0 ? examNameParts[0].Trim() : "Exam Schedule";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule details");
                ErrorMessage = "An unexpected error occurred while retrieving schedule details.";
                return Page();
            }
        }

        private async Task<SchedulingResult> GetScheduleDetailsAsync(int scheduleId)
        {
            try
            {
                var client = CreateHttpClient();
                var response = await client.GetAsync($"api/examschedule/{scheduleId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<SchedulingResult>(content, _jsonOptions);
                }

                _logger.LogWarning("API returned non-success status code: {StatusCode}", response.StatusCode);
                return new SchedulingResult { Success = false, Message = await response.Content.ReadAsStringAsync() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule details");
                return new SchedulingResult { Success = false, Message = "An error occurred while communicating with the API" };
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

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            // Implement PDF export functionality here
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            // Implement Excel export functionality here
            return RedirectToPage();
        }
    }
}
