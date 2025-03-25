using System.Text;
using System.Text.Json;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FAPCLClient.Pages.BookingManagement
{
    public class CreateBookingModel(HttpClient httpClient) : PageModel
    {
        private const string ApiBaseUrl = "http://localhost:5043/api";

        [BindProperty(SupportsGet = true)]
        public int RoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SlotId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.Now.Date;

        public Room RoomDetails { get; set; } = new Room();
        public Slot SlotDetails { get; set; } = new Slot();
        public string? Token { get; set; }

        [BindProperty]
        public string Purpose { get; set; } = string.Empty; 

        public async Task<IActionResult> OnGet()
        {
            Token = HttpContext.Session.GetString("Token");
            
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            if (string.IsNullOrEmpty(Token))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Gọi API lấy thông tin phòng
            var roomResponse = await httpClient.GetAsync($"{ApiBaseUrl}/Room/admin/room/{RoomId}");
            if (roomResponse.IsSuccessStatusCode)
            {
                RoomDetails = await roomResponse.Content.ReadFromJsonAsync<Room>() ?? new Room();
            }
            else
            {
                return NotFound("Room not found");
            }

            // Gọi API lấy thông tin slot
            var slotResponse = await httpClient.GetAsync($"{ApiBaseUrl}/Slot/{SlotId}");
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
            Token = HttpContext.Session.GetString("Token");
            
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            if (string.IsNullOrEmpty(Token))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var newBooking = new
            {
                RoomId,
                SlotId,
                SelectedDate,
                Purpose
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(newBooking), Encoding.UTF8, "application/json");

            // Gọi API để tạo booking
            var response = await httpClient.PostAsync($"{ApiBaseUrl}/Booking/createBooking", jsonContent);

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