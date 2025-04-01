using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FAPCLClient.Pages;

public class ResendConfirmationEmailModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;

    public ResendConfirmationEmailModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [BindProperty]
    public string Email { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    public void OnGet()
    {
        // Hiển thị khi người dùng truy cập lần đầu
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Email))
        {
            ErrorMessage = "Please enter your email address.";
            return Page();
        }

        try
        {
            // Gọi API gửi lại email xác nhận
            var client = _clientFactory.CreateClient();
            var response = await client.PostAsync("http://localhost:5043/api/User/resend-confirmation-email", new StringContent(
                $"{{ \"Email\": \"{Email}\" }}", Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "A new confirmation email has been sent!";
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