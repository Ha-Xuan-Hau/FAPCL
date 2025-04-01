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
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexModel> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public string? Token { get; set; }
        public bool IsAdmin { get; set; }

        public List<ExamListItem> Exams { get; set; } = new List<ExamListItem>();
        public string ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = GetQuarterStartDate(DateTime.Today);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = GetQuarterEndDate(DateTime.Today);


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
                        isAdmin = roleClaim?.Value == "Admin";
                    }
                }

                IsAdmin = isAdmin;
    

                if (!IsAdmin)
                {
                    ErrorMessage = "You don't have permission to view the exam list.";
                    Exams = new List<ExamListItem>(); // Empty list
                    return Page();
                }

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
            // Hardcode the base URL
            var baseUrl = "http://localhost:5043/api/";
            client.BaseAddress = new Uri(baseUrl);

            // Get token from session instead of claims
            if (!string.IsNullOrEmpty(Token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            }
            return client;
        }



        #endregion

        private static DateTime GetQuarterStartDate(DateTime date)
        {
            int quarterStartMonth = ((date.Month - 1) / 4) * 4 + 1;
            return new DateTime(date.Year, quarterStartMonth, 1);
        }

        private static DateTime GetQuarterEndDate(DateTime date)
        {
            int endMonth = ((date.Month - 1) / 4) * 4 + 4;
            int daysInMonth = DateTime.DaysInMonth(date.Year, endMonth);
            return new DateTime(date.Year, endMonth, daysInMonth);
        }
    }
}
