using exam_frontend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Unicode;
using exam_admin.Models;
using exam_admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace exam_admin.Pages.Account
{
    public class Login : PageModel
    {
        [BindProperty] public UserLoginModel Model { get; set; }
        private readonly ILogger<Login> logger;
        public string ErrorMessage { get; set; }

        private readonly IAuthService auth_service;

        public Login(ILogger<Login> logger, IAuthService authService)
        {
            auth_service = authService;
            this.logger = logger;
        }

        public class LoginInput
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
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
                string token = result.Message;
                HttpContext.Session.SetString("jwt", result.Message);
                var parameteres = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            "3fd00454580de44ea216d8b7b234267a2a6a6aec7e56d2b38e641a45597af0f2"u8.ToArray()),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameteres, out var securityToken);
            
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            
                return RedirectToPage("/Index");
            }
            ErrorMessage = result.Message;
            return Page();
        }
    }
}

