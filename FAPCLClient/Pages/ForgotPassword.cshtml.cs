using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FAPCLClient.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public ForgotPasswordModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public string Email { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet()
        {
            // Không cần xử lý gì trong OnGet
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "Please enter your email address.";
                return Page();
            }

            // Gửi yêu cầu đến API để reset mật khẩu
            var client = _clientFactory.CreateClient();
            var requestData = new
            {
                Email = this.Email
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestData),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await client.PostAsync("http://localhost:5043/api/User/reset-password-request", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "A reset password link has been sent to your email.";
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ErrorMessage = "Error sending email: " + errorResponse;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred: " + ex.Message;
            }

            return Page();
        }
    }
}
