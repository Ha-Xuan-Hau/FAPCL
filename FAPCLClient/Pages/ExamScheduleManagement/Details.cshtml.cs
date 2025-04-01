using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using FAPCLClient.Model;
using FAPCLClient.Model.DTOs;
using FAPCL.DTO.ExamSchedule;
using System.IdentityModel.Tokens.Jwt;

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
                // Get token from session
                Token = HttpContext.Session.GetString("Token");

                // Initialize role flags
                bool isAdmin = false;
                bool isTeacher = false;
                bool hasAccess = false;
                string userId = string.Empty;

                // Extract information from JWT token
                if (!string.IsNullOrEmpty(Token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(Token) as JwtSecurityToken;

                    if (jsonToken != null)
                    {
                        // Look for role claims
                        var roleClaim = jsonToken.Claims.FirstOrDefault(c =>
                            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                            c.Type == "role");

                        if (roleClaim != null)
                        {
                            string roleValue = roleClaim.Value;
                            Console.WriteLine($"Role from JWT token: '{roleValue}'");

                            // Check for specific roles
                            isAdmin = roleValue.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                            isTeacher = roleValue.Equals("Teacher", StringComparison.OrdinalIgnoreCase);
                            hasAccess = isAdmin || isTeacher;
                        }

                        // Extract user ID from token
                        var userIdClaim = jsonToken.Claims.FirstOrDefault(c =>
                            c.Type == "nameid" ||
                            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                        if (userIdClaim != null)
                        {
                            userId = userIdClaim.Value;
                            Console.WriteLine($"User ID from JWT token: '{userId}'");
                        }
                    }
                }

                // Now use the extracted values
                if (!hasAccess)
                {
                    ErrorMessage = "You don't have permission to view this page.";
                    return Page();
                }

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
