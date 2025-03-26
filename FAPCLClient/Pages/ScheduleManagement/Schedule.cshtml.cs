using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FAPCL.DTO;

namespace FAPCLClient.Pages.ScheduleManagement
{
    public class ScheduleModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public ScheduleModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7007");
        }

        [BindProperty(SupportsGet = true)]
        public string SelectedWeek { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedYear { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public List<ScheduleEntryDto> Schedules { get; set; }

        public async Task OnGet()
        {
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

            var response = await _httpClient.GetAsync($"/api/schedule?fromDateMonth={FromDate}&toDateMonth={ToDate}&Year={year}");

            if (response.IsSuccessStatusCode)
            {
                Schedules = await response.Content.ReadFromJsonAsync<List<ScheduleEntryDto>>();
            }
            else
            {
                Schedules = new List<ScheduleEntryDto>();
            }
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