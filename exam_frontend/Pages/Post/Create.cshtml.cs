using System.Net.Http.Headers;
using System.Security.Claims;
using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Post;

[Authorize]
public class Create : PageModel
{
    private readonly IApiService api;

    public Create(IApiService api)
    {
        this.api = api;
    }

    [BindProperty] public CreatePostModel Model { get; set; }
    public int SelectedGalleryId { get; set; }
    public IEnumerable<PreviewGalleryModel> Galleries { get; set; }
    

    public bool CanUpload { get; set; }

    public async Task<IActionResult> OnGetAsync(int gallery_id)
    {
        // Entities.Gallery gallery =
        //     await gallery_service.GetUserGallery(User.FindFirstValue(ClaimTypes.NameIdentifier)!, gallery_id);
        //
        // Entities.Gallery gallery = null;
        // HttpResponseMessage response = await api.GetAsync($"Galleries/{gallery_id}");
        // if (response.IsSuccessStatusCode)
        // {
        //     gallery = api.JsonToContent<Entities.Gallery>(await response.Content.ReadAsStringAsync());
        // }
        //
        // if (gallery == null)
        // {
        //     CanUpload = false;
        // }
        // else
        // {
        //     CanUpload = true;
        //     GalleryId = gallery_id;
        // }

        Galleries = await api.GetWithContentAsync<IList<PreviewGalleryModel>>
            ($"Galleries/{User.FindFirstValue(ClaimTypes.NameIdentifier)}");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Model.File == null || Model.File.Length == 0)
        {
            ModelState.AddModelError("ImageFile", "Please select a file.");
            return Page();
        }
        
        String[] allowed_types = { "image/jpeg", "image/png" };
        if (!Array.Exists(allowed_types, type => type == Model.File.ContentType))
        {
            ModelState.AddModelError("ImageFile", "Only JPEG and PNG images are allowed.");
            return Page();
        }
        
        const int max_size = 10 * 1024 * 1024; // 20MB
        if (Model.File.Length > max_size)
        {
            ModelState.AddModelError("ImageFile", "File size must be under 20MB.");
            return Page();
        }
        
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(Model.Name), "Name");
        content.Add(new StringContent(Model.Description), "Description");
        content.Add(new StringContent(Model.GalleryId.ToString()), "GalleryId");
        
        var fileContent = new StreamContent(Model.File.OpenReadStream());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(Model.File.ContentType);
        content.Add(fileContent, "File", Model.File.FileName);

        await api.PostAsync("Posts/", content);
        //await image_service.PostImage(ImageFile, GalleryId);

        return RedirectToPage("/Gallery/Details", 
            new { user_id = User.FindFirstValue(ClaimTypes.NameIdentifier), gallery_id = SelectedGalleryId });
    }

}