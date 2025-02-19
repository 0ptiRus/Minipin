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
    public List<Entities.Image> Images { get; set; }

    public Details(GalleryService service)
    {
        this.service = service;
    }

    public async void OnGet(int gallery_id)
    {
        Entities.Gallery gallery = await service.GetGalleryWithImages(User.FindFirstValue(ClaimTypes.NameIdentifier)!
            ,gallery_id);
        GalleryName = gallery.Name;
        Images = gallery.Images.ToList();
    }
}