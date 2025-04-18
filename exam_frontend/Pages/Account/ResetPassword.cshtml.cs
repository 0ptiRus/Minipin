using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Threading.Tasks;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account
{
    public class ResetPassword : PageModel
    {
        private readonly IApiService api;
        
        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        [BindProperty]
        public ResetPasswordModel Model { get; set; }

        public ResetPassword(IApiService api)
        {
            this.api = api;
        }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(Email))
            {
                TempData["ErrorMessage"] = "Invalid reset link.";
                return RedirectToPage("/Error");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var request = new
            {
                Email = Email,
                ResetCode = Token, 
                NewPassword = Model.NewPassword
            };

            HttpResponseMessage response = await api.PostAsJsonAsync("User/reset-password", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your password has been successfully reset!";
                return RedirectToPage("/Account/Login");
            }

            ErrorResponse? error_response = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            TempData["ErrorMessage"] = error_response?.Message ?? "An error occurred while resetting your password.";
            return Page();
        }
    }
}