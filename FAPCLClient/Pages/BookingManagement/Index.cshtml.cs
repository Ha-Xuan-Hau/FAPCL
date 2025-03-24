using System.Text.Json;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookClassRoom.Pages.BookingManagement
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public List<Room> FilteredRooms { get; set; } = new();
        public List<Slot> Slots { get; set; } = new();
        public List<RoomType> RoomTypes { get; set; } = new();

        [BindProperty(SupportsGet = true)] public DateTime SelectedDate { get; set; } = DateTime.Now.Date;
        [BindProperty(SupportsGet = true)] public int? RoomTypeId { get; set; }
        [BindProperty(SupportsGet = true)] public bool? HasProjector { get; set; }
        [BindProperty(SupportsGet = true)] public bool? HasSoundSystem { get; set; }

        public Dictionary<(int RoomId, int SlotId), bool> SlotAvailability { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 6;

        private const string ApiBaseUrl = "http://localhost:5043/api";

        public async Task OnGetAsync(int currentPage = 1)
        {
            if (SelectedDate < DateTime.Now.Date)
            {
                SelectedDate = DateTime.Now.Date;
            }

            await LoadRoomDataAsync(currentPage);
            await LoadSlotsDataAsync();
            await LoadRoomTypesDataAsync();
            await LoadSlotAvailabilityAsync();

            CurrentPage = currentPage < 1 ? 1 : Math.Min(currentPage, TotalPages);
        }

        private async Task LoadRoomDataAsync(int currentPage)
        {
            string url = $"{ApiBaseUrl}/Room/rooms?selectedDate={SelectedDate:yyyy-MM-dd}" +
                         $"&roomTypeId={RoomTypeId}&hasProjector={HasProjector}" +
                         $"&hasSoundSystem={HasSoundSystem}&currentPage={currentPage}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RoomApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null)
                {
                    FilteredRooms = result.Rooms;
                    TotalPages = result.TotalPages;
                }
            }
        }

        private async Task LoadSlotsDataAsync()
        {
            string url = $"{ApiBaseUrl}/Slot";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonSlots = await response.Content.ReadAsStringAsync();
                Slots = JsonSerializer.Deserialize<List<Slot>>(jsonSlots, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        private async Task LoadRoomTypesDataAsync()
        {
            string url = $"{ApiBaseUrl}/RoomType";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonRoomTypes = await response.Content.ReadAsStringAsync();
                RoomTypes = JsonSerializer.Deserialize<List<RoomType>>(jsonRoomTypes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        private async Task LoadSlotAvailabilityAsync()
        {
            foreach (var room in FilteredRooms)
            {
                foreach (var slot in Slots)
                {
                    SlotAvailability[(room.RoomId, slot.SlotId)] = await IsSlotAvailableAsync(room.RoomId, slot.SlotId, SelectedDate);
                }
            }
        }

        public async Task<bool> IsSlotAvailableAsync(int roomId, int slotId, DateTime selectedDate)
        {
            string url = $"{ApiBaseUrl}/Room/availability?roomId={roomId}&slotId={slotId}&selectedDate={selectedDate:yyyy-MM-dd}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SlotAvailabilityResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.IsAvailable ?? false;
            }
            return false;
        }

        public class RoomApiResponse
        {
            public List<Room> Rooms { get; set; } = new();
            public int TotalPages { get; set; }
        }

        public class SlotAvailabilityResponse
        {
            public bool IsAvailable { get; set; }
        }
    }
}
