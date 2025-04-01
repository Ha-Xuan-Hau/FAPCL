using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FAPCLClient.Pages.NewsManagement
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<IdentityUser> _userManager;
        private const string ApiBaseUrl = "http://localhost:5043/api/News"; // Thay bằng URL API của bạn
        public string? Token { get; set; }
        [BindProperty]
        public NewsDTO News { get; set; } = new NewsDTO();

        public CreateModel(HttpClient httpClient, UserManager<IdentityUser> userManager)
        {
            _httpClient = httpClient;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Token = HttpContext.Session.GetString("Token");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            if (string.IsNullOrEmpty(Token))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            // Khởi tạo giá trị mặc định (nếu cần)
            News.isPublished = true;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Token = HttpContext.Session.GetString("Token");
            string userId="";
            if (!string.IsNullOrEmpty(Token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(Token);

                // Lấy UserId từ claim "sub" hoặc "nameidentifier"
                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            }


            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Gán thông tin người tạo
            News.createdby = userId;

            try
            {
                // Gọi API để tạo tin
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(News),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{ApiBaseUrl}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "News created successfully!";
                    return RedirectToPage("./Index");
                }

                // Xử lý lỗi từ API
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to create news: {errorContent}";
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return Page();
            }
        }
    }
}