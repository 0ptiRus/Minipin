using exam_admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_admin.Pages
{
    [Authorize]
    public class CommentsModel : PageModel
    {
        public string Filter { get; set; } = null;
        public IList<SidebarItem> Items { get; set; }
        public CommentsModel()
        {
            Items = new List<SidebarItem>
            {
                new SidebarItem
                {
                    Text = "All comments",
                    Icon = "fas fa-comments mr-2",

                },
                new SidebarItem
                {
                    Text = "Flagged comments",
                    Icon = "fas fa-flag mr-2",

                },
                new SidebarItem
                {
                    Text = "Deleted comments",
                    Icon = "fas fa-trash mr-2",

                }
            };
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
}
