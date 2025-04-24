using System.Net;
using System.Security.Claims;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public readonly IApiService api_service;
    public readonly MinioService minio;
    public IList<Entities.Gallery> Galleries { get; set; } = new List<Entities.Gallery>();

    public IndexModel(MinioService minio, IApiService api_service)
    {
        this.minio = minio;
        this.api_service = api_service;
    }

    public async Task<IActionResult> OnGet()
    {
        //Galleries = await service.GetFeed(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user_id = User.FindFirstValue("nameid");
        HttpResponseMessage response =  await api_service.GetAsync($"Galleries/feed/{user_id}");
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return RedirectToPage("/Account/Login");
        }

        if (await response.Content.ReadAsStringAsync() != "")
        {
            Galleries = api_service.JsonToContent<IList<Entities.Gallery>>(await response.Content.ReadAsStringAsync());   
        }
        return Page();
    }
}