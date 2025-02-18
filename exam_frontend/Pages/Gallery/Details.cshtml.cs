using System.Security.Claims;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Gallery;

[Authorize]
public class Details : PageModel
{
    private readonly GalleryService service;
    public string GalleryName { get; set; }
    public List<string> Urls { get; set; }

    public Details(GalleryService service)
    {
        this.service = service;
    }

    public async void OnGet(int gallery_id)
    {
        Entities.Gallery gallery = await service.GetGalleryWithImages(User.FindFirstValue(ClaimTypes.NameIdentifier)!
            ,gallery_id);
        GalleryName = gallery.Name;
        Urls = new(gallery.Images.Select(i => i.FilePath));
    }
}