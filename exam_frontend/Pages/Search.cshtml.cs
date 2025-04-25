using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages;

public class Search : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Query { get; set; }
    public void OnGet()
    {
        
    }
}