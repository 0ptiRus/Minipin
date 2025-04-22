using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class Logout : PageModel
{
    public async Task<IActionResult> OnPost()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync();
        return RedirectToPage("/Account/Login");
    }
}