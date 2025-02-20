using System.Security.Claims;
using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

[Authorize]
public class AddFollow : PageModel
{
    private readonly UserManager<ApplicationUser> manager;
    private readonly FollowService service;

    public AddFollow(UserManager<ApplicationUser> manager, FollowService service)
    {
        this.manager = manager;
        this.service = service;
    }

    [BindProperty]
    public string SearchUsername { get; set; }

    public UserModel? FoundUser { get; set; }
    public string Message { get; set; } = "";

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(SearchUsername))
        {
            Message = "Please enter a username.";
            return Page();
        }
        
        ApplicationUser result = await manager.FindByNameAsync(SearchUsername);

        if (result == null)
        {
            Message = "User not found.";
        }

        FoundUser = new(result.Id, result.UserName);

        return Page();
    }

    public async Task<IActionResult> OnPostFollow(string user_id_to_follow)
    {
        string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (await service.PostFollower(user_id, user_id_to_follow) != null)
        {
            Message = "You are now following this user!";
        }
        else
        {
            Message = "Failed to follow user.";
        }

        return Page();
    }

}