using System.Security.Claims;
using exam_frontend.Controllers;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace exam_frontend.Pages.Gallery;

[Authorize]
public class Details : PageModel
{
    private readonly GalleryService service;
    private readonly ImageService image_service;

    public string GalleryName { get; set; }
    public List<Entities.Image> Images { get; set; }
    public string UserId { get; set; }
    
    public int GalleryId { get; set; }

    public Details(GalleryService service, ImageService image_service)
    {
        this.service = service;
        this.image_service = image_service;
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

    public async Task<IActionResult> OnPostDeleteAsync(int imageId)
    {
        if(await image_service.DeleteImage(imageId))
            return RedirectToPage("/Gallery/Details", new { user_id = UserId ,gallery_id = GalleryId });
        return BadRequest();
    }
}