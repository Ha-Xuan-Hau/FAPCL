
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FAPCLClient.Model;
using FAPCL.DTO.ExamSchedule;

namespace FAPCLClient.Pages.ExamScheduleManagement
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateModel> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        [BindProperty]
        public string ExamType { get; set; }

        [BindProperty]
        public string ExamName { get; set; }

        [BindProperty]
        public List<int> SelectedCourseIds { get; set; } = new List<int>();

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        public SelectList AvailableCourses { get; set; }
        public string ErrorMessage { get; set; }

        public CreateModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<CreateModel> logger)
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
                // Set initial values
                StartDate = DateTime.Today;
                EndDate = DateTime.Today.AddDays(13);

                // Load courses
                var courses = await GetCoursesAsync();
                if (courses == null)
                {
                    return Page();
                }
                AvailableCourses = new SelectList(courses, "CourseId", "CourseName");

                // Ensure at least one course selection field
                if (SelectedCourseIds.Count == 0)
                {
                    SelectedCourseIds.Add(0);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading exam scheduling creation page");
                ErrorMessage = "Failed to load page data. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCoursesData();
                return Page();
            }

            try
            {
                // Validate date range
                if (StartDate > EndDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date");
                    await LoadCoursesData();
                    return Page();
                }

                TimeSpan duration = EndDate - StartDate;
                if (duration.TotalDays > 14) // 14 days inclusive
                {
                    ModelState.AddModelError("EndDate", "Exam period cannot exceed 14 days");
                    await LoadCoursesData();
                    return Page();
                }

                // Filter out empty course selections
                SelectedCourseIds = SelectedCourseIds.Where(id => id != 0).ToList();

                if (!SelectedCourseIds.Any())
                {
                    ModelState.AddModelError("SelectedCourseIds", "Please select at least one course");
                    await LoadCoursesData();
                    return Page();
                }

                // Create and send request to API
                var request = new ExamScheduleRequest
                {
                    ExamName = ExamName,
                    CourseIds = SelectedCourseIds,
                    StartDate = StartDate,
                    EndDate = EndDate
                };

                var result = await ScheduleExamsAsync(request);

                if (result == null || !result.Success)
                {
                    ErrorMessage = result?.Message ?? "Failed to schedule exams. Please try again.";
                    await LoadCoursesData();
                    return Page();
                }

                // Redirect to details page
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling exams");
                ErrorMessage = "An unexpected error occurred while scheduling exams.";
                await LoadCoursesData();
                return Page();
            }
        }

        private async Task LoadCoursesData()
        {
            var courses = await GetCoursesAsync();
            AvailableCourses = new SelectList(courses ?? new List<CourseDTO>(), "CourseId", "CourseName");
        }

        #region API Calls

        private async Task<List<CourseDTO>> GetCoursesAsync()
        {
            try
            {
                var client = CreateHttpClient();
                var response = await client.GetAsync("ExamSchedule/currentSemeterCourses");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<CourseDTO>>(content, _jsonOptions);
                }

                ErrorMessage = $"Failed to load courses: {response.ReasonPhrase}";
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses");
                ErrorMessage = "Failed to load courses. Please try again.";
                return null;
            }
        }

        private async Task<SchedulingResult> ScheduleExamsAsync(ExamScheduleRequest request)
        {
            try
            {
                var client = CreateHttpClient();

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("ExamSchedule", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<SchedulingResult>(responseContent, _jsonOptions);
                }

                ErrorMessage = $"API error: {responseContent}";
                return new SchedulingResult { Success = false, Message = responseContent };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling exams");
                return new SchedulingResult { Success = false, Message = "An error occurred while communicating with the API" };
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
