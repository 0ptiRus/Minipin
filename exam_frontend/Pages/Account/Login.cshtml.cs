using exam_frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Login : PageModel
{
    [BindProperty]
    public UserLoginModel Model { get; set; }

    private readonly IAuthService auth_service;
    public string ErrorMessage { get; set; }

    public Login(IAuthService authService)
    {
        auth_service = authService;
    }

    public void OnGet()
    {
        
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        LoginResponse result = await auth_service.LoginAsync(Model);
        if (result.Message == "OK" && result is not null)
        {
            return RedirectToPage("/Index");
        }
        else
        {
            ErrorMessage = "Login failed. Please check your credentials.";
            return Page();
        }

    }
}