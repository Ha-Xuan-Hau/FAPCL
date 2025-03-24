using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FAPCLClient.Model;

namespace BookClassRoom.Pages.BookingManagement
{
    public class BookingDetailModel : PageModel
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        
        private const string ApiBaseUrl = "http://localhost:5043/api/Booking";

        public BookingDetailModel(UserManager<AspNetUser> userManager, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _userManager = userManager;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;

            ConfirmedBookings = new List<Booking>();
            CompleteBookings = new List<Booking>();
            AllBookings = new List<Booking>();
            Rooms = new List<Room>();
            SearchQuery = string.Empty;
        }

        public string UserId { get; set; }
        public bool IsAdmin { get; set; }
        public List<Booking> ConfirmedBookings { get; set; }
        public List<Booking> CompleteBookings { get; set; }
        public List<Booking> AllBookings { get; set; }
        public List<Room> Rooms { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 6;
        public const int PageSize2 = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public async Task OnGet(int currentPage = 1)
        {
            UserId = _userManager.GetUserId(User);
            IsAdmin = User.IsInRole("Admin");

            if (IsAdmin)
            {
                var url = $"{ApiBaseUrl}/details?currentPage={currentPage}&searchQuery={SearchQuery}";
                var result = await _httpClient.GetFromJsonAsync<PagedResult<Booking>>(url);

                if (result != null)
                {
                    AllBookings = result.Items;
                    TotalPages = result.TotalPages;
                    CurrentPage = result.CurrentPage;
                }
            }
            else
            {
                var confirmedUrl = $"{ApiBaseUrl}/confirmed?searchQuery={SearchQuery}";
                ConfirmedBookings = await _httpClient.GetFromJsonAsync<List<Booking>>(confirmedUrl) ?? new List<Booking>();

                var completedUrl = $"{ApiBaseUrl}/completed?searchQuery={SearchQuery}&currentPage={currentPage}";
                var result = await _httpClient.GetFromJsonAsync<PagedResult<Booking>>(completedUrl);

                if (result != null)
                {
                    CompleteBookings = result.Items;
                    TotalPages = result.TotalPages;
                    CurrentPage = result.CurrentPage;
                }
            }
        }

        public async Task<IActionResult> OnPostCancelBooking(int bookingId)
        {
            var cancelUrl = $"{ApiBaseUrl}/cancel";
            
            var response = await _httpClient.PostAsJsonAsync(cancelUrl, new { BookingId = bookingId });

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }

            return BadRequest("Failed to cancel booking.");
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}