using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace exam_admin.Pages
{
    [Authorize(Policy = "AdminOnly")]
    public class CommentsModel : PageModel
    {
        private readonly AppDbContext context;

        public CommentsModel(AppDbContext context)
        {
            this.context = context;
        }

        public List<Comment> Comments { get; set; }

        public async Task OnGetAsync()
        {
            Comments = await context.Comments
                .Include(c => c.User)
                .ToListAsync();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int commentId)
        {
            Comment? comment = await context.Comments.FindAsync(commentId);

            if (comment != null)
            {
                context.Comments.Remove(comment);
                await context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
