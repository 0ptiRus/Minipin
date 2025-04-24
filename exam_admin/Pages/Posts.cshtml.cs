using exam_admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_admin.Pages;

public class Posts : PageModel
{
    public string Filter { get; set; } = null;
    public List<SidebarItem> Items { get; set; }

    public Posts()
    {
        Items = new List<SidebarItem>
        {
            new SidebarItem
            {
                Text = "All posts",
                Icon = "fas fa-thumbtack mr-2",
                OnClick =  nameof(OnPostAll).Remove(0, 6)

            },
            new SidebarItem
            {
                Text = "Flagged content",
                Icon = "fas fa-flag mr-2",
                OnClick =  nameof(OnPostFlagged).Remove(0, 6)
            },
            new SidebarItem
            {
                Text = "Deleted content",
                Icon = "fas fa-trash mr-2",
                OnClick = nameof(OnPostDeleted).Remove(0, 6)
            }
        };
    }
    
    public void OnGet()
    {
        
    }

    public IActionResult OnPostAll()
    {
        Filter = null;
        return Page();
    }

    public IActionResult OnPostFlagged()
    {
        Filter = "flagged";
        return Page();
    }

    public IActionResult OnPostDeleted()
    {
        Filter = "deleted";
        return Page();
    }
    
}