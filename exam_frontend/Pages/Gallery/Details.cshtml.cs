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
    public string UserId { get; set; }
    
    public int GalleryId { get; set; }

    public Details(GalleryService service)
    {
        this.service = service;
    }

    public async void OnGet(string user_id, int gallery_id)
    {
        Entities.Gallery gallery = await service.GetGalleryWithImages(user_id
            ,gallery_id);
        GalleryName = gallery.Name;
        GalleryId = gallery_id;
        UserId = user_id;
        Images = gallery.Images.ToList();
    }
}