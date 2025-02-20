using System.Security.Claims;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

[Authorize]
public class Followers : PageModel
{
    private readonly FollowService service;

    public Followers(FollowService service)
    {
        this.service = service;
    }

    public IList<UserModel> Follows { get; set; }
    public IList<UserModel> Followed { get; set; }
    
    [BindProperty] public string FollowerIdToRemove { get; set; }
    [BindProperty] public string FollowingIdToRemove { get; set; }

    public async Task<IActionResult> OnGet()
    {
        string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Follows = await service.GetFollowers(user_id);
        Followed = await service.GetFollowed(user_id);
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveFollower()
    {
        bool IsDeleted = await service.DeleteFollower(FollowerIdToRemove, User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (IsDeleted)
        {
            return RedirectToPage();
        }

        return BadRequest();
    }

    public async Task<IActionResult> OnPostUnfollow()
    {
        bool IsDeleted = await service.DeleteFollower(User.FindFirstValue(ClaimTypes.NameIdentifier), FollowingIdToRemove);
        if (IsDeleted)
        {
            return RedirectToPage();
        }

        return BadRequest();
    }

}