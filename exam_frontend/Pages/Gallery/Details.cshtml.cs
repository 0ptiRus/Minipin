using System.Security.Claims;
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

    public string GalleryName { get; set; }
    public List<Entities.Post> Posts { get; set; } = new();
    public string UserId { get; set; }
    public readonly IApiService api;
    
    public int GalleryId { get; set; }

    public Details(MinioService minio, IApiService api)
    {
        this.minio = minio;
        this.api = api;
    }

    public async void OnGet(string user_id, int gallery_id)
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
            response = await api.GetAsync($"Galleries/images/{user_id}/{gallery_id}");
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
            gallery = JsonConvert.DeserializeObject<Entities.Gallery>(await response.Content.ReadAsStringAsync()); 
            GalleryName = gallery.Name;
            GalleryId = gallery_id;
            UserId = user_id;
            Posts = gallery.Posts.ToList();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int imageId)
    {
        HttpResponseMessage response = await api.PostAsync($"Images/delete?id={imageId}", null);
        // if(await image_service.DeleteImage(imageId))
        //     return RedirectToPage("/Gallery/Details", new { user_id = UserId ,gallery_id = GalleryId });
        return BadRequest();
    }
}