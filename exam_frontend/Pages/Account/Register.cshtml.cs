using exam_frontend.Entities;
using exam_frontend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Register : PageModel
{
    private readonly UserManager<ApplicationUser> user_manager;
    public string ErrorMessage { get; set; }
    [BindProperty]
    public RegisterModel Model { get; set; }

    public Register(UserManager<ApplicationUser> manager)
    {
        user_manager = manager;
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

        ApplicationUser user = new ApplicationUser { UserName = Model.Email, Email = Model.Email, EmailConfirmed = true};
        IdentityResult result = await user_manager.CreateAsync(user, Model.Password);

        if (result.Succeeded)
        {
            return RedirectToPage("/Account/Login");
        }
        else
        {
            ErrorMessage = result.Errors.ToString();
            return Page();
        }
    }
}