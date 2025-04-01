using FAPCL.DTO.ExamSchedule;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAPCLClient.Pages.ExamScheduleManagement
{
    public class TeacherScheduleExamsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ScheduleExamsModel> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public string? Token { get; set; }
        public bool IsTeacher { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TeacherId { get; set; }

        public List<StudentExamScheduleDTO> ExamSchedules { get; set; }
        public string ErrorMessage { get; set; }



        public TeacherScheduleExamsModel(
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
            bool isTeacher = false;
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

                    Console.WriteLine($"Role from JWT token: '{roleClaim?.Value}'");
                    isTeacher = roleClaim?.Value == "Teacher";

                    // Extract user ID from the token
                    var userIdClaim = jsonToken.Claims.FirstOrDefault(c =>
                        c.Type == "nameid" ||
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" ||
                        c.Type == "sub");

                    if (userIdClaim != null)
                    {
                        userId = userIdClaim.Value;
                        Console.WriteLine($"User ID from JWT token: '{userId}'");

                        // Optionally store in session for easier access later
                        HttpContext.Session.SetString("UserId", userId);
                    }
                }
            }

            TeacherId = userId.Trim();
            IsTeacher = isTeacher;

            if (!IsTeacher)
            {
                ErrorMessage = "Only teacher can access this page.";
                return Page();
            }
            try
            {
                var client = CreateHttpClient();
                var response = await client.GetAsync($"ExamSchedule/teacher/{TeacherId}");
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

            if (!string.IsNullOrEmpty(Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
            return client;
        }


    }
}
