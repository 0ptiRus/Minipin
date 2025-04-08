using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Register : PageModel
{
    public string ErrorMessage { get; set; }
    [BindProperty]
    public UserRegisterModel Model { get; set; }

    private IAuthService auth_service;

    public Register(IAuthService auth_service)
    {
        this.auth_service = auth_service;
    }

    public void OnGet()
    {
        
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine(Model.Pfp.FileName);
        if (!ModelState.IsValid)
        {
            Console.WriteLine("Invalid model");
            return Page();
        }

        RegisterResponse result = await auth_service.RegisterAsync(Model);

        if (result.IsCreated)
        {
            return RedirectToPage("/Account/Login");
        }
        else
        {
            Console.WriteLine(result.Message);
            ErrorMessage = result.Message;
            return Page();
        }
    }
}