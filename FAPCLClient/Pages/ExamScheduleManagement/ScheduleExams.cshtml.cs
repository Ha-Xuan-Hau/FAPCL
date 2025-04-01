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
        public bool IsAdmin { get; set; }

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
                    isAdmin = roleClaim?.Value == "Student";
                }
            }
            IsAdmin = isAdmin;
            // Check if user is a student
            if (!IsAdmin)
            {
                ErrorMessage = "Only students can access this page.";
                return Page();
            }

            // Get user ID from session
            var userId = HttpContext.Session.GetString("UserId");

            // If UserId is not set, extract it from the token
            // if (string.IsNullOrEmpty(userId))
            // {
            //     userId = GetUserIdFromToken(Token);
            //     if (string.IsNullOrEmpty(userId))
            //     {
            //         ErrorMessage = "Could not determine your student ID. Please log in again.";
            //         return Page();
            //     }

            //     // Save it in session for future use
            //     HttpContext.Session.SetString("UserId", userId);
            // }

            StudentId = userId.Trim();

            try
            {
                var client = CreateHttpClient();
                // Call API endpoint to get exam schedule for the student
                var response = await client.GetAsync($"ExamSchedule/student/{StudentId}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Failed to load exam schedules: {response.ReasonPhrase}";
                    return Page();
                }

                var content = await response.Content.ReadAsStringAsync();
                ExamSchedules = JsonSerializer.Deserialize<List<StudentExamScheduleDTO>>(content, _jsonOptions);

                // Check if ExamSchedules is null after deserialization
                if (ExamSchedules == null)
                {
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

        // private string GetUserIdFromToken(string token)
        // {
        //     try
        //     {
        //         var handler = new JwtSecurityTokenHandler();
        //         var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        //         if (jsonToken == null)
        //             return string.Empty;

        //         // Look for the userId claim in the token
        //         var userIdClaim = jsonToken.Claims.FirstOrDefault(claim =>
        //             claim.Type == "nameid" ||
        //             claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        //         return userIdClaim?.Value ?? string.Empty;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error parsing JWT token");
        //         return string.Empty;
        //     }
        // }

    }
}
