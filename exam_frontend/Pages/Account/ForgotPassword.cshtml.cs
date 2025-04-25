using exam_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class ForgotPassword : PageModel
{
    private readonly IApiService api;

    [BindProperty]
    public string Email { get; set; }

    public ForgotPassword(IApiService api)
    {
        this.api = api;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var request = new { Email };
        
        HttpResponseMessage response = await api.PostAsJsonAsync("Account/forgot-password", request);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "If the email exists, a reset link will be sent.";
            return RedirectToPage("/Account/ForgotPassword");
        }
        
        ErrorResponse? error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        TempData["ErrorMessage"] = error?.Message ?? "An error occurred while processing your request.";
        return RedirectToPage("/Account/ForgotPassword");
    }
}

public class ErrorResponse
{
    public string Message { get; set; }
}