using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IApiService service;

    public IndexModel(ILogger<IndexModel> logger, IApiService service)
    {
        _logger = logger;
        this.service = service;
    }

    public async Task OnGet()
    {
       
    }
}