using FAPCL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FAPCLClient.Pages.NewsManagement
{
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:5043/api/News";
        public string? Token { get; set; }

        [BindProperty]
        public NewsDTO News { get; set; } = new NewsDTO();
        public int id;

        public EditModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            this.id = id;
            Token = HttpContext.Session.GetString("Token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            if (string.IsNullOrEmpty(Token))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var response = await _httpClient.GetFromJsonAsync<NewsDTO>($"{ApiBaseUrl}/managerNews/{id}");

            if (response == null)
            {
                return NotFound();
            }

            News = response;
            return Page();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(int id)
        {
            this.id = id;
            Token = HttpContext.Session.GetString("Token");
            string userId = "";

            if (!string.IsNullOrEmpty(Token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(Token);
                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
                

            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(News),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PutAsync($"{ApiBaseUrl}/{id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "News updated successfully!";
                    return RedirectToPage("./Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Failed to update news: {errorContent}";
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
