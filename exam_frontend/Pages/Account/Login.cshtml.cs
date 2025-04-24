using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace exam_frontend.Pages.Account;

public class Login : PageModel
{
    [BindProperty]
    public UserLoginModel Model { get; set; }

    public string ErrorMessage { get; set; }

    private readonly IAuthService auth_service;

    public Login(IAuthService auth_service)
    {
        this.auth_service = auth_service;
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
        if (result.StatusCode == 200)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwt = handler.ReadJwtToken(result.Message);
            var claims = jwt.Claims;
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(30), // Set expiration date to 7 days from now
                Path = "/", // Cookie is available within the entire application
                Secure = true, // Ensure the cookie is only sent over HTTPS
                HttpOnly = true, // Prevent client-side scripts from accessing the cookie
                IsEssential = true // Indicates the cookie is essential for the application to function
            };
            
            HttpContext.Response.Cookies.Append("jwt", result.Message, options);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            
            return RedirectToPage("/Index");
        }

        ErrorMessage = result.Message;
        return Page();

    }
}