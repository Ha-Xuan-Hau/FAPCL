using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using FAPCLClient.Model;
using FAPCLClient.Model.DTOs;
using FAPCL.DTO.ExamSchedule;

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

        public List<DetailedExamInfo> ExamInfos { get; set; }
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
                _logger.LogInformation($"Retrieving exam schedule details for ID: {Id}");

                var result = await GetScheduleDetailsAsync(Id);

                if (result == null)
                {
                    ErrorMessage = "Failed to retrieve schedule details due to a system error.";
                    _logger.LogWarning("GetScheduleDetailsAsync returned null");
                    return Page();
                }

                if (!result.Success)
                {
                    // Clean up the error message if it contains the raw API response
                    if (result.Message.Contains("API Error: NotFound"))
                    {
                        ErrorMessage = "The requested exam schedule could not be found.";
                    }
                    else
                    {
                        ErrorMessage = result.Message;
                    }

                    _logger.LogWarning($"Failed to retrieve schedule: {ErrorMessage}");
                    return Page();
                }

                ExamInfos = result.DetailedExam;

                if (ExamInfos == null || !ExamInfos.Any())
                {
                    ErrorMessage = "No exam information available for this schedule.";
                    return Page();
                }

                // Extract the exam name from the first exam
                var firstExam = ExamInfos.First();
                var examNameParts = firstExam.ExamName?.Split("[Session:");
                ExamName = examNameParts?.Length > 0 ? examNameParts[0].Trim() : "Exam Schedule";

                _logger.LogInformation($"Successfully retrieved details for {ExamInfos.Count} exam(s)");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule details");
                ErrorMessage = "An unexpected error occurred while retrieving schedule details.";
                return Page();
            }
        }

        #region API Calls
        private async Task<DetailedExamResult> GetScheduleDetailsAsync(int scheduleId)
        {
            try
            {
                var client = CreateHttpClient();
                var response = await client.GetAsync($"ExamSchedule/{scheduleId}");

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"API Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<DetailedExamResult>(content, _jsonOptions);
                    return result;
                }

                // Special handling for NotFound (404) errors
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new DetailedExamResult
                    {
                        Success = false,
                        Message = "The requested exam schedule could not be found. It may have been deleted or never existed."
                    };
                }

                _logger.LogWarning($"API returned non-success status code: {response.StatusCode}");
                return new DetailedExamResult
                {
                    Success = false,
                    Message = $"API Error: {response.StatusCode} - {content}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule details");
                return new DetailedExamResult
                {
                    Success = false,
                    Message = $"Communication error: {ex.Message}"
                };
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
