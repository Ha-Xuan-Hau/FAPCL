using BookClassRoom.Hubs;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;

namespace FAPCLClient.Pages.RoomTypeManagement
{
    public class RoomTypeModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        public RoomType RoomType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public RoomTypeModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5043/api/RoomType/");
        }

        public List<RoomType> RoomTypes { get; set; } = new List<RoomType>();

        public async Task OnGetAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var roomTypes = JsonSerializer.Deserialize<List<RoomType>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    RoomTypes = roomTypes ?? new List<RoomType>();

                    if (!string.IsNullOrEmpty(SearchQuery))
                    {
                        RoomTypes = RoomTypes.Where(c => c.RoomType1.Contains(SearchQuery)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        public async Task<IActionResult> OnPostCreate(RoomType roomType)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
            
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var json = JsonSerializer.Serialize(roomType);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể thêm RoomType.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tạo RoomType: " + ex.Message);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdate(RoomType roomType)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
            
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var json = JsonSerializer.Serialize(roomType);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{roomType.RoomTypeId}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Không thể cập nhật RoomType {roomType.RoomTypeId}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật RoomType: " + ex.Message);
            }

            return Page();
        }
    }
}