using System.Security.Claims;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace exam_frontend.Pages.Gallery;

[Authorize]
public class Details : PageModel
{
    public readonly MinioService minio;
    public readonly IApiService api;
    public GalleryDetailsModel Model { get; set; }
    
    public int GalleryId { get; set; }

    public Details(MinioService minio, IApiService api)
    {
        this.minio = minio;
        this.api = api;
    }

    public async Task OnGet(string user_id, int gallery_id)
    {
        // Entities.Gallery gallery = await service.GetGalleryWithImages(user_id
        //     ,gallery_id);
        // GalleryName = gallery.Name;
        // GalleryId = gallery_id;
        // UserId = user_id;
        // Images = gallery.Images.ToList()
        Entities.Gallery gallery;
        HttpResponseMessage response;
        try
        {
            response = await api.GetAsync($"Galleries/{gallery_id}");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request was canceled: {ex.Message}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request error: {ex.Message}");
            throw;
        }
        if (response.IsSuccessStatusCode)
        {
            Model = JsonConvert.DeserializeObject<GalleryDetailsModel>(await response.Content.ReadAsStringAsync()); 
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int imageId)
    {
        HttpResponseMessage response = await api.PostAsJsonAsync($"Images/delete?id={imageId}", null);
        // if(await image_service.DeleteImage(imageId))
        //     return RedirectToPage("/Gallery/Details", new { user_id = UserId ,gallery_id = GalleryId });
        return BadRequest();
    }
}