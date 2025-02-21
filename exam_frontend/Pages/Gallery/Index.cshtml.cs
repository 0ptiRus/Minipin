using System.Security.Claims;
using exam_frontend.Entities;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace exam_frontend.Pages.Gallery;

[Authorize]
public class Index : PageModel
{
    private readonly GalleryService service;

    public bool IsOwner { get; set; }

    public Index(GalleryService service)
    {
        this.service = service;
    }

    public IList<Entities.Gallery> Galleries { get; set; }

    public async Task<IActionResult> OnGetAsync(string user_id)
    {
        IsOwner = User.FindFirstValue(ClaimTypes.NameIdentifier) == user_id;
        Galleries = await service.GetUserGalleries(user_id);
        return Page();
    }


    public async Task<IActionResult> OnPostDeleteAsync(int galleryId)
    {
        if(await service.DeleteGallery(galleryId)) return Page();
        return BadRequest();
    }
}