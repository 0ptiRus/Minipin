using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace exam_admin.Pages
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> user_manager;
        private readonly ILogger<UsersModel> logger;

        public UsersModel(UserManager<ApplicationUser> userManager, ILogger<UsersModel> logger)
        {
            user_manager = userManager;
            this.logger = logger;
        }

        public List<ApplicationUser> Users { get; set; }

        public async Task OnGet()
        {
            Users = await user_manager.Users.ToListAsync();
        }

        public async Task<string> GetUserRole(ApplicationUser user)
        {
            IList<string> roles = await user_manager.GetRolesAsync(user);
            return roles.Any() ? string.Join(", ", roles) : "User";
        }

        public async Task<IActionResult> OnPostBan(string userId)
        {
            ApplicationUser? user = await user_manager.FindByIdAsync(userId);
            if (user != null)
            {
                await user_manager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                logger.LogInformation($"User {user.UserName} was banned.");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMakeAdmin(string userId)
        {
            ApplicationUser? user = await user_manager.FindByIdAsync(userId);
            if (user != null)
            {
                await user_manager.AddToRoleAsync(user, "Admin");
                logger.LogInformation($"User {user.UserName} was promoted to Admin.");
            }
            return RedirectToPage();
        }
    }

}
