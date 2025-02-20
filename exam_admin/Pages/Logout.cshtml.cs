using exam_frontend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_admin.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signin_manager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            signin_manager = signInManager;
        }

        public async Task<IActionResult> OnPost()
        {
            await signin_manager.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }
    }
}
