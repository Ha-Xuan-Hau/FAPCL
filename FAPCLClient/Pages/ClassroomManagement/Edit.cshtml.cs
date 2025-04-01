using System.Text;
using System.Text.Json;
using BookClassRoom.Hubs;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace FAPCLClient.Pages.ClassroomManagement
{
    public class EditModel : PageModel
    {
        private readonly IHubContext<SignalRServer> _hubContext;
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api";

        public EditModel(IHubContext<SignalRServer> hubContext, IHttpClientFactory httpClientFactory)
        {
            _hubContext = hubContext;
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty]
        public Room Room { get; set; } = new Room();

        public SelectList RoomTypeOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Room/admin/room/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            Room = await response.Content.ReadFromJsonAsync<Room>();
            
            string urlRoomTypes = $"{ApiBaseUrl}/RoomType";
            
            var responseRoomTypes = await _httpClient.GetFromJsonAsync<List<RoomType>>(urlRoomTypes);
            if (responseRoomTypes != null)
            {
                RoomTypeOptions = new SelectList(responseRoomTypes, "RoomTypeId", "RoomType1");
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var jsonContent = new StringContent(JsonSerializer.Serialize(Room), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{ApiBaseUrl}/admin/room/{Room.RoomId}", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to update room");
            }
            await _hubContext.Clients.All.SendAsync("LoadRoom");
            return RedirectToPage("/ClassroomManagement/Index");
        }
    }
}