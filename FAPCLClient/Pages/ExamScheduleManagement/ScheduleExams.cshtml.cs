using FAPCL.DTO.ExamSchedule;
using FAPCLClient.Model;
using FAPCLClient.Model.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAPCLClient.Pages.ExamScheduleManagement
{
    public class ScheduleExamsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ScheduleExamsModel> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        public string? Token { get; set; }
        public bool IsStudent { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StudentId { get; set; }

        public List<StudentExamScheduleDTO> ExamSchedules { get; set; }
        public string ErrorMessage { get; set; }

        public ScheduleExamsModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ScheduleExamsModel> logger)
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
            Token = HttpContext.Session.GetString("Token");
            // Extract claims from JWT token
            bool isStudent = false;
            string userId = null;

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
                    isStudent = roleClaim?.Value == "Student";

                    // Extract user ID from the token
                    var userIdClaim = jsonToken.Claims.FirstOrDefault(c =>
                        c.Type == "nameid" ||
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" ||
                        c.Type == "sub");

                    if (userIdClaim != null)
                    {
                        userId = userIdClaim.Value;
                        HttpContext.Session.SetString("UserId", userId);
                    }
                }
            }

            StudentId = userId.Trim();
            IsStudent = isStudent;

            if (!IsStudent)
            {
                ErrorMessage = "Only student can access this page.";
                return Page();
            }

            try
            {
                var client = CreateHttpClient();
                var response = await client.GetAsync($"ExamSchedule/student/{StudentId}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Failed to load exam schedules: {response.ReasonPhrase}";
                    return Page();
                }
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw JSON response: {content}");
                try
                {
                    // Make sure your JSON options include case insensitivity
                    ExamSchedules = JsonSerializer.Deserialize<List<StudentExamScheduleDTO>>(content, _jsonOptions);

                    // Check after deserialization
                    if (ExamSchedules == null || !ExamSchedules.Any())
                    {
                        ErrorMessage = "No exam schedules found.";
                        return Page();
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                    ErrorMessage = "Failed to deserialize exam schedules. Please try again.";
                    return Page();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exam schedules");
                ErrorMessage = "An error occurred while loading exam schedules.";
                return Page();
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

    }
}
