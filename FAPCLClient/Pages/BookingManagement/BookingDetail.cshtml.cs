using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FAPCLClient.Pages.BookingManagement
{
    public class BookingDetailModel : PageModel
    {
        private readonly HttpClient _httpClient;

        private const string ApiBaseUrl = "http://localhost:5043/api/Booking";

        public BookingDetailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();

            ConfirmedBookings = new List<Booking>();
            CompleteBookings = new List<Booking>();
            AllBookings = new List<Booking>();
            Rooms = new List<Room>();
            SearchQuery = string.Empty;
        }

        public string? Token { get; set; }
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
            Token = HttpContext.Session.GetString("Token");
            IsAdmin = User.IsInRole("Admin");
            
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

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
            
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            
            var response = await _httpClient.PostAsJsonAsync(cancelUrl, new { BookingId = bookingId });

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }

            return BadRequest("Failed to cancel booking.");
        }
    }

    public class PagedResult<T>(int totalPages, int currentPage)
    {
        public List<T> Items { get; set; } = new();
        public int TotalPages { get; set; } = totalPages;
        public int CurrentPage { get; set; } = currentPage;
    }
}