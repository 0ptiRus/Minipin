using exam_frontend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace exam_admin.Pages;

public class ImagesModel : PageModel
{
    private readonly AppDbContext context;

    public ImagesModel(AppDbContext context)
    {
        this.context = context;
    }

    public IList<Image> Images { get; set; }

    public async Task OnGetAsync()
    {
        Images = await context.Images.ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var image = await context.Images.FindAsync(id);
        if (image == null)
        {
            return NotFound();
        }
        // System.IO.File.Delete(image.FilePath);
        context.Images.Remove(image);
        await context.SaveChangesAsync();

        return RedirectToPage();
    }

}