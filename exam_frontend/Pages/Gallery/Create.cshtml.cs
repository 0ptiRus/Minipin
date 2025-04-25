using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using exam_frontend.Entities;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Gallery;

[Authorize]
public class Create : PageModel
{
    private readonly IApiService api;

    public Create(IApiService api)
    {
        this.api = api;
    }

    [BindProperty]
    public CreateGalleryModel Model { get; set; }
    
    [BindProperty]
    public string Privacy { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Page();
        }
        //await service.CreateGallery(new(Model.Name, User.FindFirstValue(ClaimTypes.NameIdentifier)!, 
        //Model.IsPrivate));

        if (Privacy == "public")
            Model.IsPrivate = false;
        else 
            Model.IsPrivate = true;
        
        Model.UserId = User.FindFirstValue("nameid");
        
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(Model.Name), "Name");
        content.Add(new StringContent(Model.Description), "Description");
        content.Add(new StringContent(Model.IsPrivate.ToString()), "IsPrivate");
        content.Add(new StringContent(Model.UserId), "UserId");

        if (Model.Image != null)
        {
            var fileContent = new StreamContent(Model.Image.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(Model.Image.ContentType);
            content.Add(fileContent, "Image", Model.Image.FileName);
        }
        
        HttpResponseMessage response = await api.PostAsync($"Galleries/", content);
        int id = 0;
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return RedirectToPage("/Account/Login");
        }
        if(response.IsSuccessStatusCode)
            id = api.JsonToContent<int>(await response.Content.ReadAsStringAsync());

        return RedirectToPage("/Gallery/Details", new { user_id = User.FindFirstValue("nameid"), gallery_id = id});
    }

    public class CreateGalleryModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPrivate { get; set; }
        public IFormFile Image { get; set; }
        public string UserId { get; set; }
    }

}