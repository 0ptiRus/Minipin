using System.Security.Claims;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages;
[Authorize]
public class IndexModel : PageModel
{
    private readonly GalleryService service;
    public IList<Entities.Gallery> Galleries { get; set; }

    public IndexModel(GalleryService service)
    {
        this.service = service;
    }

    public async Task<IActionResult> OnGet()
    {
        Galleries = await service.GetFeed(User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Page();
    }
}