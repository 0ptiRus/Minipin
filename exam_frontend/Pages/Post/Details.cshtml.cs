using System.Security.Claims;
using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Post;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using exam_frontend.Entities;

[Authorize]
public class Details : PageModel
{
    private readonly IApiService api;
    [BindProperty] public PostModel Post { get; set; }

    public Details(IApiService api, MinioService minio)
    {
        this.api = api;
    }

    public async Task OnGet(int post_id)
    {
        //Image = await image_service.GetImage(image_id);
        HttpResponseMessage response = await api.GetAsync($"Posts/{post_id}");
        if(response.IsSuccessStatusCode)
            Post = api.JsonToContent<PostModel>(await response.Content.ReadAsStringAsync());
    }
    
}

