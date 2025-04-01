using BookClassRoom.Hubs;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace FAPCLClient.Pages.ClassroomManagement
{
    public class IndexModel : PageModel
    {
        private readonly IHubContext<SignalRServer> _hubContext;
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api";

        public IndexModel(IHubContext<SignalRServer> hubContext, HttpClient httpClient)
        {
            _hubContext = hubContext;
            _httpClient = httpClient;
        }

        public IList<Room> Room { get; set; } = new List<Room>();
        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 6;

        [BindProperty(SupportsGet = true)]
        public int? RoomTypeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasProjector { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? HasSoundSystem { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoomName { get; set; }

        public async Task OnGetAsync(int currentPage = 1)
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "roomTypeId", RoomTypeId?.ToString() },
                { "hasProjector", HasProjector?.ToString() },
                { "hasSoundSystem", HasSoundSystem?.ToString() },
                { "roomName", RoomName },
                { "currentPage", currentPage.ToString() }
            };
            
            string queryString = string.Join("&", queryParams.Where(q => q.Value != null).Select(q => $"{q.Key}={q.Value}"));
            string url = $"{ApiBaseUrl}/Room/rooms?{queryString}";
            
            var response = await _httpClient.GetFromJsonAsync<RoomResponse>(url);
            
            string urlRoomTypes = $"{ApiBaseUrl}/RoomType";
            
            var responseRoomTypes = await _httpClient.GetFromJsonAsync<List<RoomType>>(urlRoomTypes);
            if (responseRoomTypes != null)
            {
                RoomTypes = responseRoomTypes;
            }
            
            if (response != null)
            {
                Room = response.Rooms;
                TotalPages = response.TotalPages;
                CurrentPage = currentPage;
            }

            await _hubContext.Clients.All.SendAsync("LoadRoom");
        }

        public async Task<IActionResult> OnPost(int roomId)
        {
            var response = await _httpClient.PutAsync($"{ApiBaseUrl}/Room/admin/room/{roomId}", null);
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            
            await _hubContext.Clients.All.SendAsync("LoadRoom");
            return RedirectToPage();
        }
    }

    public class RoomResponse
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public int TotalPages { get; set; }
    }
}
