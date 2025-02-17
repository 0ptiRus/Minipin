using exam_frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Register : PageModel
{
    private readonly IAuthService service;
    public string ErrorMessage { get; set; }
    [BindProperty]
    public UserRegisterModel Model { get; set; }

    public Register(IAuthService service)
    {
        this.service = service;
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

        RegisterResponse response = await service.RegisterAsync(Model);

        if (response.IsCreated == true)
        {
            return RedirectToPage("/Account/Login");
        }
        else
        {
            ErrorMessage = response.Message;
            return Page();
        }
    }
}