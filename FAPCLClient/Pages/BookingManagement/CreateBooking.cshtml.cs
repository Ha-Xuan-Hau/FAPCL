using FAPCLClient.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookClassRoom.Pages.BookingManagement
{
    public class CreateBookingModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<AspNetUser> _userManager;
        
        private const string ApiBaseUrl = "http://localhost:5043/api";

        [BindProperty(SupportsGet = true)]
        public int RoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SlotId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.Now.Date;

        public Room RoomDetails { get; set; } = new Room();
        public Slot SlotDetails { get; set; } = new Slot();
        public string UserId { get; set; }

        [BindProperty]
        public string Purpose { get; set; }

        public CreateBookingModel(HttpClient httpClient, UserManager<AspNetUser> userManager)
        {
            _httpClient = httpClient;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet()
        {
            UserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Gọi API lấy thông tin phòng
            var roomResponse = await _httpClient.GetAsync($"{ApiBaseUrl}/Room/{RoomId}");
            if (roomResponse.IsSuccessStatusCode)
            {
                RoomDetails = await roomResponse.Content.ReadFromJsonAsync<Room>() ?? new Room();
            }
            else
            {
                return NotFound("Room not found");
            }

            // Gọi API lấy thông tin slot
            var slotResponse = await _httpClient.GetAsync($"{ApiBaseUrl}/Slot/{SlotId}");
            if (slotResponse.IsSuccessStatusCode)
            {
                SlotDetails = await slotResponse.Content.ReadFromJsonAsync<Slot>() ?? new Slot();
            }
            else
            {
                return NotFound("Slot not found");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            UserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var newBooking = new
            {
                RoomId = RoomId,
                SlotId = SlotId,
                SelectedDate = SelectedDate,
                UserId = UserId,
                Purpose = Purpose
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(newBooking), Encoding.UTF8, "application/json");

            // Gọi API để tạo booking
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Booking/createBooking", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Booking Successfully!";
                return RedirectToPage("/BookingManagement/Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to create booking!";
                return Page();
            }
        }
    }
}