using System.Net;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Account;

public class UserList : PageModel
{
    public string CurrentUserId { get; set; }
    public string ViewedUserId { get; set; }
    public bool IsFollowers { get; set; }
    [BindProperty] public UserListModel Model { get; set; }
    [BindProperty] public bool ShowFollowers { get; set; } = true;

    private readonly IApiService api;

    public UserList(IApiService api)
    {
        this.api = api;
    }
    
    public async Task OnGet(string user_id, string viewed_user_id, bool is_followers)
    {
        CurrentUserId = user_id;
        ViewedUserId = viewed_user_id;
        IsFollowers = is_followers;

        HttpResponseMessage response = await api.GetAsync(
            $"Follows/follow_list?user_id={ViewedUserId}&viewing_user_id={CurrentUserId}");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        { 
            RedirectToPage("/Account/Login");
        }
        
        if (response.IsSuccessStatusCode)
        {
            Model = api.JsonToContent<UserListModel>(await response.Content.ReadAsStringAsync());
            ShowFollowers = IsFollowers;   
        }
    }
    
    
    public IActionResult OnPostSetTab(bool showFollowers)
    {
        ShowFollowers = showFollowers;
        return new JsonResult(new { success = true });
    }

    public string GetTabClass(bool isFollowersTab)
    {
        var active = ShowFollowers == isFollowersTab;
        return $"px-4 py-3 font-medium {(active ? "tab-active" : "text-gray-500 hover:text-gray-800")}";
    }
}