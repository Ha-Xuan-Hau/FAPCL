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
        public bool IsAdmin { get; set; }

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
            // Extract role from JWT token
            bool isAdmin = false;
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
                    isAdmin = roleClaim?.Value == "Teacher";
                }
            }

            IsAdmin = isAdmin;
            // Check if user is a student
            if (!IsAdmin)
            {
                ErrorMessage = "Only teacher can access this page.";
                return Page();
            }

            var userId = HttpContext.Session.GetString("UserId");

            TeacherId = userId.Trim();
            try
            {
                var client = CreateHttpClient();
                // Call API endpoint to get exam schedule for the student
                var response = await client.GetAsync($"ExamSchedule/teacher/{TeacherId}");
                //06c924c7-80ab-4b70-9842-9464dbcffb37
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Failed to load exam schedules: {response.ReasonPhrase}";
                    return Page();
                }

                if (ExamSchedules == null)
                {
                    ErrorMessage = "Failed to deserialize exam schedules. Please try again.";
                    return Page();
                }

                var content = await response.Content.ReadAsStringAsync();
                ExamSchedules = JsonSerializer.Deserialize<List<StudentExamScheduleDTO>>(content, _jsonOptions);

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
