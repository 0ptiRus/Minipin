using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace exam_admin.Pages
{
    [Authorize(Policy = "AdminOnly")]
    public class GalleriesModel : PageModel
    {
        private readonly AppDbContext context;
        private readonly ILogger<GalleriesModel> logger;

        public GalleriesModel(AppDbContext context, ILogger<GalleriesModel> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public List<Gallery> Galleries { get; set; }

        public async Task OnGet()
        {
            Galleries = await context.Galleries.Include(g => g.User).ToListAsync();
        }

        public async Task<IActionResult> OnPostDelete(int galleryId)
        {
            Gallery gallery = await context.Galleries.FindAsync(galleryId);
            if (gallery != null)
            {
                context.Galleries.Remove(gallery);
                await context.SaveChangesAsync();
                logger.LogInformation($"Gallery {gallery.Name} was deleted.");
            }
            return RedirectToPage();
        }
    }

}
