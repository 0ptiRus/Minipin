using exam_admin.Models;
using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace exam_admin.Pages
{
    [Authorize]
    public class UsersModel : PageModel
    {
        private readonly ILogger<UsersModel> logger;
        public List<SidebarItem> Items { get; set; }
        public string Filter { get; set; } = null;
 
        public UsersModel(ILogger<UsersModel> logger)
        {
            this.logger = logger;
            Items = new()
            {
                new SidebarItem
                {
                    Text = "All Users",
                    Icon = "fas fa-users",
                    OnClick =  nameof(OnPostAll).Remove(0, 6),
                },
                new SidebarItem { Text = "Administrators", Icon = "fas fa-user-shield", OnClick = nameof(OnPostAdmin).Remove(0, 6)},
                new SidebarItem { Text = "Banned Users", Icon = "fas fa-user-lock", OnClick =  nameof(OnPostBanned).Remove(0, 6)}
            };
        }

        public async Task OnGet()
        {
         
        }
        
        public IActionResult OnPostAll()
        {
            Filter = null;
            return Page();
        }

        public IActionResult OnPostAdmin()
        {
            Filter = "administrators";
            return Page();
        }

        public IActionResult OnPostBanned()
        {
            Filter = "banned";
            return Page();
        }
        
    }

}
