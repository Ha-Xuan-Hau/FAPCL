using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FAPCLClient.Model;
using System.Text.Json.Serialization;

namespace FAPCLClient.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly BookClassRoomContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(HttpClient httpClient, BookClassRoomContext context, ILogger<LoginModel> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var loginData = new
            {
                email = Input.Email,
                password = Input.Password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:5043/api/User/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();

                // Deserialize token từ API
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseData);

                // Tìm User trong Database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return Page();
                }
                
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => _context.Roles.FirstOrDefault(r => r.Id == ur.RoleId).Name)
                    .ToListAsync();

                // Kiểm tra nếu token đã tồn tại
                var existingToken = await _context.AspNetUserTokens
                    .FirstOrDefaultAsync(t => t.UserId == user.Id && t.LoginProvider == "JWT" && t.Name == "AccessToken");

                if (existingToken != null)
                {
                    _context.AspNetUserTokens.Remove(existingToken);
                }

                // Tạo một UserToken mới và lưu vào DB
                var userToken = new IdentityUserToken<string>
                {
                    UserId = user.Id,
                    LoginProvider = "JWT",
                    Name = "AccessToken",
                    Value = tokenResponse.token
                };

                // Thêm vào DB
                _context.AspNetUserTokens.Add(userToken);
                // Lưu thông tin người dùng vào Session
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("Role", string.Join(",", userRoles));
                HttpContext.Session.SetString("Token", tokenResponse.token);

                await _context.SaveChangesAsync();

                _logger.LogInformation("User logged in successfully and token stored.");
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            else
            {
                // Đọc thông báo lỗi từ API
                var responseData = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseData);

                if (errorResponse != null && errorResponse.ContainsKey("message"))
                {
                    string message = errorResponse["message"];
                    Console.WriteLine($"Error: {message}");
                    ModelState.AddModelError(string.Empty, message); // Thêm vào ModelState nếu cần
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An unknown error occurred.");
                }
            }
            return Page();
        }

        public class TokenResponse
        {
            [JsonPropertyName("token")]
            public string token { get; set; }

        }

    }
}
