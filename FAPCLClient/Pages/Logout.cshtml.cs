using Microsoft.AspNetCore.Mvc.RazorPages;

public class LogoutModel : PageModel
{
    public void OnGet()
    {
        HttpContext.Session.Clear();
        RedirectToPage("/");
    }
}