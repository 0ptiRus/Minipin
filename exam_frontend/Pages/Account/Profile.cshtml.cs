using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Profile : PageModel
{
    [BindProperty]
    public ProfileViewModel Model { get; set; }
    [BindProperty] public int GalleryId { get; set; }
    public readonly IApiService api;

    public Profile(IApiService api)
    {
        this.api = api;
    }

    public async Task OnGet(string profile_user_id)
    {
        Model = await api.GetWithContentAsync<ProfileViewModel>($"User/profile?user_id={User.FindFirstValue("nameid")}&profile_user_id={profile_user_id}");
    }

    public async Task<IActionResult> OnPost()
    {
        RemoveGalleryModel model = new RemoveGalleryModel
        {
            Id = GalleryId
        };
        
        HttpResponseMessage response = await api.PostAsJsonAsync("Galleries/delete", model);
        if (response.IsSuccessStatusCode)
            return RedirectToPage();
        return BadRequest();
    }

    public async Task<IActionResult> OnPostFollow(string followed_user_id)
    {
        HttpResponseMessage response = await api.PostAsJsonAsync($"Follows/follow?user_id={User.FindFirstValue("nameid")}&followed_user_id={followed_user_id}", null);
        if (response.IsSuccessStatusCode)
        {
            Model = await api.GetWithContentAsync<ProfileViewModel>(
                $"User/profile?user_id={User.FindFirstValue("nameid")}&profile_user_id={followed_user_id}"
            );

            return Page();   
        }
        return BadRequest();
    }
    
    public async Task<IActionResult> OnPostUnfollow(string followed_user_id)
    {
        HttpResponseMessage response = await api.PostAsJsonAsync($"Follows/unfollow?user_id={User.FindFirstValue("nameid")}&followed_user_id={followed_user_id}", null);
        if (response.IsSuccessStatusCode)
        {
            Model = await api.GetWithContentAsync<ProfileViewModel>(
                $"User/profile?user_id={User.FindFirstValue("nameid")}&profile_user_id={followed_user_id}"
            );

            return Page();   
        }
        return BadRequest();
    }
}