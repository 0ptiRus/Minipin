using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using exam_api.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class Profile : PageModel
{
    [BindProperty]
    public ProfileViewModel Model { get; set; }
    public readonly IApiService api;

    public Profile(IApiService api)
    {
        this.api = api;
    }

    public async Task OnGet(string user_id)
    {
        Console.WriteLine(User.FindFirstValue(ClaimTypes.NameIdentifier));
        Model = await api.GetWithContentAsync<ProfileViewModel>($"User/profile/{user_id}");
    }
}