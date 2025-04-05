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
    public readonly MinioService minio;
    public readonly IApiService api;

    public bool IsOwner { get; set; }
    public int PinsAmount { get; set; }

    public Index(MinioService minio, IApiService api)
    {
        this.minio = minio;
        this.api = api;
    }

    public IList<Entities.Gallery> Galleries { get; set; }

    public async Task<IActionResult> OnGetAsync(string user_id)
    {
        IsOwner = User.FindFirstValue(ClaimTypes.NameIdentifier) == user_id;
        //Galleries = await service.GetUserGalleries(user_id);
        HttpResponseMessage response = await api.GetAsync($"Galleries/{user_id}");
        if (response.IsSuccessStatusCode)
        {
            Galleries = api.JsonToContent<List<Entities.Gallery>>(await response.Content.ReadAsStringAsync());
            foreach (Entities.Gallery gallery in Galleries)
            {
                PinsAmount += gallery.Posts.Count;
            }
        }
        return Page();
    }


    public async Task<IActionResult> OnPostDeleteAsync(int galleryId)
    {
        //if(await service.DeleteGallery(galleryId)) return RedirectToPage();
        return BadRequest();
    }
}