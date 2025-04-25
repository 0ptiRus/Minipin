using System.Net;
using System.Security.Claims;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public readonly IApiService api_service;
    public IList<PostModel> Posts { get; set; } = new List<PostModel>();

    public IndexModel(IApiService api_service)
    {
        this.api_service = api_service;
    }

    public async Task OnGet()
    {
        //Galleries = await service.GetFeed(User.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}