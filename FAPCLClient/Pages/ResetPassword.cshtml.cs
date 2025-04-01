using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FAPCLClient.Pages;

public class ResetPasswordModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;

    public ResetPasswordModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [BindProperty]
    public string Password { get; set; }

    [BindProperty]
    public string ConfirmPassword { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    // Bind giá trị từ form (Hidden fields)
    [BindProperty]
    public string UserId { get; set; }

    [BindProperty]
    public string Token { get; set; }

    public void OnGet(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            ErrorMessage = "Invalid or expired reset password link.";
            return;
        }
        UserId = userId;
        Token = token;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        // Gửi yêu cầu đến API để reset mật khẩu
        var client = _clientFactory.CreateClient();

        var requestData = new
        {
            userId = UserId,
            token = Token,
            newPassword = Password,
            confirmPassword = ConfirmPassword
        };

        var jsonContent = new StringContent(
            JsonConvert.SerializeObject(requestData),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await client.PostAsync("http://localhost:5043/api/User/reset-password", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Your password has been reset successfully.";
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                ErrorMessage = errorResponse;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred: " + ex.Message;
        }

        return Page();
    }
}
