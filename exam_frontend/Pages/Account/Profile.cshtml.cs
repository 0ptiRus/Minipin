using exam_api.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Profile : PageModel
{
    public ProfileViewModel Model { get; set; }
    public readonly IApiService api;
    
    public async void OnGet(string user_id)
    {
        Model = await api.GetWithContentAsync<ProfileViewModel>($"Users/profile/{user_id}");
    }
}