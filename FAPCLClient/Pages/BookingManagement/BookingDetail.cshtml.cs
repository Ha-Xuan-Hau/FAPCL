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

            ConfirmedBookings = new List<BookingDTO>();
            CompleteBookings = new List<BookingDTO>();
            AllBookings = new List<BookingDTO>();
            Rooms = new List<Room>();
            SearchQuery = string.Empty;
        }

        public string? Token { get; set; }
        public bool IsAdmin { get; set; }
        public List<BookingDTO> ConfirmedBookings { get; set; }
        public List<BookingDTO> CompleteBookings { get; set; }
        public List<BookingDTO> AllBookings { get; set; }
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

            try
            {
                if (IsAdmin)
                {
                    var url = $"{ApiBaseUrl}/details?currentPage={currentPage}&searchQuery={SearchQuery}";
                    var result = await _httpClient.GetFromJsonAsync<List<BookingDTO>>(url);

                    if (result != null)
                    {
                        AllBookings = result;
                    }
                }
                else
                {
                    var confirmedUrl = $"{ApiBaseUrl}/confirmed?searchQuery={SearchQuery}";
                    ConfirmedBookings = await _httpClient.GetFromJsonAsync<List<BookingDTO>>(confirmedUrl) ?? new List<BookingDTO>();

                    var completedUrl = $"{ApiBaseUrl}/completed?searchQuery={SearchQuery}&currentPage={currentPage}";
                    var result = await _httpClient.GetFromJsonAsync<List<BookingDTO>>(completedUrl);

                    if (result != null)
                    {
                        CompleteBookings = result;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log the exception (you can use a logging framework)
                Console.Error.WriteLine($"Request error: {ex.Message}");

                // Handle the error (e.g., set an error message to display in the UI)
                ModelState.AddModelError(string.Empty, "An error occurred while fetching booking data. Please try again later.");
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
    
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string? Purpose { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime SlotBookingDate { get; set; }
        public string? Status { get; set; }
        public string RoomName { get; set; }
        public string SlotNumber { get; set; }
    }
}