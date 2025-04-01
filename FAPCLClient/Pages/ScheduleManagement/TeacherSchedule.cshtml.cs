using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FAPCL.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FAPCLClient.Pages.ScheduleManagement
{
    public class TeacherScheduleModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public TeacherScheduleModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty(SupportsGet = true)]
        public string SelectedWeek { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedYear { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public List<TeacherScheduleDto> Schedule { get; set; } = new List<TeacherScheduleDto>();

        private (string Id, string Role) GetInfoFromToken()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return (string.Empty, string.Empty);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var Id = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            var role = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(Id))
            {
                RedirectToPage("/Account/Login");
                return (string.Empty, string.Empty);
            }

            return (Id, role);
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = GetInfoFromToken().Id;
            var role = GetInfoFromToken().Role;
            if (string.IsNullOrEmpty(studentId))
            {
                return Redirect("~/Identity/Account/Login");
            }
            if (role != "Teacher")
            {
                return RedirectToPage("/Index");
            }
            DateTime today = DateTime.Now;
            int currentYear = today.Year;

            if (string.IsNullOrEmpty(SelectedYear))
            {
                SelectedYear = currentYear.ToString();
            }

            int year = int.Parse(SelectedYear);

            if (string.IsNullOrEmpty(SelectedWeek))
            {
                SelectedWeek = GetCurrentWeek(today, year).ToString();
            }

            int week = int.Parse(SelectedWeek);
            var (fromDate, toDate) = GetWeekRange(year, week);

            FromDate = fromDate.ToString("dd-MM");
            ToDate = toDate.ToString("dd-MM");

            string token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Identity/Account/Login");
            }
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"http://localhost:5043/api/schedule/teacher?fromDateMonth={FromDate}&toDateMonth={ToDate}&Year={year}");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Schedule = new List<TeacherScheduleDto>();
                return Page();
            }

            Schedule = await response.Content.ReadFromJsonAsync<List<TeacherScheduleDto>>();
            return Page();
        }


        private int GetCurrentWeek(DateTime date, int year)
        {
            DateTime firstMonday = GetFirstMondayOfYear(year);
            return (int)Math.Floor((date - firstMonday).TotalDays / 7) + 1;
        }

        private (DateTime fromDate, DateTime toDate) GetWeekRange(int year, int weekNumber)
        {
            DateTime firstMonday = GetFirstMondayOfYear(year);
            DateTime fromDate = firstMonday.AddDays((weekNumber - 1) * 7);
            DateTime toDate = fromDate.AddDays(6);

            return (fromDate, toDate);
        }

        private DateTime GetFirstMondayOfYear(int year)
        {
            DateTime firstDay = new DateTime(year, 1, 1);
            while (firstDay.DayOfWeek != DayOfWeek.Monday)
            {
                firstDay = firstDay.AddDays(1);
            }
            return firstDay;
        }

    }
}
