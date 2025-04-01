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

        public string? Token { get; set; }
        public bool IsAdmin { get; set; }

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
                Token = HttpContext.Session.GetString("Token");
                string role = HttpContext.Session.GetString("Role") ?? string.Empty;
                string userId = HttpContext.Session.GetString("UserId") ?? string.Empty;

                // Check if the user has either Admin or Teacher role
                bool isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                bool isTeacher = role.Equals("Teacher", StringComparison.OrdinalIgnoreCase);

                // Check if the user has either role
                bool hasAccess = isAdmin || isTeacher;

                if (!hasAccess)
                {
                    ErrorMessage = "You don't have permission to view the exam list.";
                    return Page();
                }

                _logger.LogInformation($"Retrieving exam schedule details for ID: {Id}");

                // Get the exam details from the API
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

                // For teachers, check if they are associated with this exam
                if (isTeacher && !string.IsNullOrEmpty(userId))
                {
                    bool teacherIsAssociated = false;

                    // Check if the teacher is associated with any of the exams in this schedule
                    foreach (var exam in ExamInfos)
                    {
                        if (exam.Teacher != null && exam.Teacher.TeacherId == userId)
                        {
                            teacherIsAssociated = true;
                            break;
                        }
                    }

                    // If the teacher is not associated with any exam in this schedule
                    if (!teacherIsAssociated)
                    {
                        ErrorMessage = "You don't have permission to view this exam schedule as you are not assigned to it.";
                        ExamInfos = null;
                        return Page();
                    }
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
            // Hardcode the base URL
            var baseUrl = "http://localhost:5043/api/";
            client.BaseAddress = new Uri(baseUrl);

            // Get token from session instead of claims
            if (!string.IsNullOrEmpty(Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
            return client;
        }


        #endregion
    }
}
