using exam_frontend.Entities;
using exam_frontend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Login : PageModel
{
    [BindProperty]
    public LoginModel Model { get; set; }

    public string ErrorMessage { get; set; }

    private readonly SignInManager<ApplicationUser> signin_manager;

    public Login(SignInManager<ApplicationUser> signInManager)
    {
        signin_manager = signInManager;
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

        var result = await signin_manager.PasswordSignInAsync(Model.Email, Model.Password, false, true);
        if (result.Succeeded)
        {
            return RedirectToPage("/Index");
        }
        else
        {  
            ErrorMessage = "Invalid login attempt.";
            return Page();
        }

    }
}