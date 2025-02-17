using exam_api.Data;
using Microsoft.AspNetCore.Mvc;

namespace exam_api.Controllers;

[Route("api/[controller]")]
public class FollowerController
{
    private readonly ApiDbContext context;

    public FollowerController(ApiDbContext context)
    {
        this.context = context;
    }
    
    
}