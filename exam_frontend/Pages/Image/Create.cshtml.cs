using System.Security.Claims;
using exam_frontend.Controllers;
using exam_frontend.Entities;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Image;

[Authorize]
public class Create : PageModel
{
    private readonly GalleryService gallery_service;
    private readonly ImageService image_service;
    private readonly IWebHostEnvironment env;

    public Create(GalleryService galleryService, ImageService imageService, IWebHostEnvironment env)
    {
        gallery_service = galleryService;
        image_service = imageService;
        this.env = env;
    }

    [BindProperty] public int GalleryId { get; set; }

    [BindProperty] public IFormFile ImageFile { get; set; }

    public bool CanUpload { get; set; }

    public async Task<IActionResult> OnGetAsync(int gallery_id)
    {
        Entities.Gallery gallery =
            await gallery_service.GetUserGallery(User.FindFirstValue(ClaimTypes.NameIdentifier)!, gallery_id);

        if (gallery == null)
        {
            CanUpload = false;
        }
        else
        {
            CanUpload = true;
            GalleryId = gallery_id;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ImageFile == null || ImageFile.Length == 0)
        {
            ModelState.AddModelError("ImageFile", "Please select a file.");
            return Page();
        }
        
        String[] allowed_types = new[] { "image/jpeg", "image/png" };
        if (!Array.Exists(allowed_types, type => type == ImageFile.ContentType))
        {
            ModelState.AddModelError("ImageFile", "Only JPEG and PNG images are allowed.");
            return Page();
        }
        
        const int max_size = 20 * 1024 * 1024; // 20MB
        if (ImageFile.Length > max_size)
        {
            ModelState.AddModelError("ImageFile", "File size must be under 20MB.");
            return Page();
        }

        await image_service.PostImage(ImageFile, GalleryId);

        return RedirectToPage("/Gallery/Index", 
            new { user_id = User.FindFirstValue(ClaimTypes.NameIdentifier) });
    }

}