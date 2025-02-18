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
    private readonly AppDbContext context;
    private readonly UserManager<ApplicationUser> user_manager;
    private readonly GalleryService service;

    public Index(AppDbContext context, UserManager<ApplicationUser> userManager, GalleryService service)
    {
        this.context = context;
        user_manager = userManager;
        this.service = service;
    }

    public IList<Entities.Gallery> Galleries { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Galleries = await service.GetUserGalleries(User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Page();
    }

}