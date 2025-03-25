using BookClassRoom.Hubs;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace FAPCLClient.Pages.ClassroomManagement
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<SignalRServer> _hubContext;
        private readonly string _apiBaseUrl = "http://localhost:5043/api"; // Thay đổi URL nếu cần

        public CreateModel(IHttpClientFactory httpClientFactory, IHubContext<SignalRServer> hubContext)
        {
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Room Room { get; set; } = new Room();

        public SelectList RoomTypeOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<List<RoomType>>($"{_apiBaseUrl}/RoomType/roomtypes");

            if (response == null)
            {
                ModelState.AddModelError("", "Không thể tải danh sách loại phòng.");
                return Page();
            }

            RoomTypeOptions = new SelectList(response, "RoomTypeId", "RoomType1");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Room.Capacity <= 0)
            {
                ModelState.AddModelError("Room.Capacity", "Capacity must be greater than 0.");
                await OnGetAsync(); // Load lại RoomTypeOptions
                return Page();
            }

            var newRoom = new Room()
            {
                RoomName = Room.RoomName,
                Capacity = Room.Capacity,
                RoomTypeId = Room.RoomTypeId,
                HasProjector = Room.HasProjector ?? false,
                HasSoundSystem = Room.HasSoundSystem ?? false,
                Status = "Available",
                IsAction = true
            };

            var token = HttpContext.Session.GetString("Token");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/Room/admin/room/add", newRoom);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Không thể thêm phòng.");
                await OnGetAsync(); // Load lại RoomTypeOptions
                return Page();
            }

            await _hubContext.Clients.All.SendAsync("LoadRoom");
            return RedirectToPage("/ClassroomManagement/Index");
        }
    }
}
