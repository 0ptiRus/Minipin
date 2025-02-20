using exam_frontend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace exam_admin.Pages.Account
{
    public class Login : PageModel
    {
        private readonly SignInManager<ApplicationUser> signin_manager;
        private readonly UserManager<ApplicationUser> user_manager;
        private readonly ILogger<Login> logger;

        public Login(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<Login> logger)
        {
            signin_manager = signInManager;
            user_manager = userManager;
            this.logger = logger;
        }

        [BindProperty]
        public LoginInput Input { get; set; }

        public string ErrorMessage { get; set; }

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

            var user = await user_manager.FindByNameAsync(Input.Username);

            if (user == null || !(await user_manager.IsInRoleAsync(user, "Admin")))
            {
                ErrorMessage = "Invalid credentials or not an admin.";
                return Page();
            }

            var result = await signin_manager.PasswordSignInAsync(Input.Username, Input.Password, true, false);

            if (result.Succeeded)
            {
                logger.LogInformation($"Admin {Input.Username} logged in.");
                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Invalid credentials.";
                return Page();
            }
        }
    }
}

