using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace FAPCLClient.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(HttpClient httpClient, ILogger<RegisterModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [Phone]
            public string PhoneNumber { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Role { get; set; }  // Role: Student or Teacher
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Kiểm tra tính hợp lệ của ModelState (Kiểm tra dữ liệu đầu vào)
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Dữ liệu đăng ký từ form người dùng
            var registerData = new
            {
                firstName = Input.FirstName,
                lastName = Input.LastName,
                email = Input.Email,
                phoneNumber = Input.PhoneNumber,
                address = Input.Address,
                password = Input.Password,
                confirmPassword = Input.ConfirmPassword,
                role = Input.Role // Gửi role vào API
            };

            // Serialize dữ liệu đăng ký thành JSON
            var json = JsonSerializer.Serialize(registerData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gửi yêu cầu đăng ký đến API
            var response = await _httpClient.PostAsync("http://localhost:5043/api/User/register", content);

            if (response.IsSuccessStatusCode)
            {
                // Lưu thông báo thành công vào TempData để hiển thị trên giao diện
                TempData["SuccessMessage"] = "User registered successfully, please check your email to confirm your account.";
                return RedirectToPage();  // Tái tải trang để hiển thị thông báo
            }
            else
            {
                // Nếu API trả về lỗi
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Registration failed: {errorContent}");
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return Page();
            }
        }




        public class TokenResponse
        {
            public string token { get; set; }
        }
    }
}
