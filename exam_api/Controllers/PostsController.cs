using exam_api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly ILogger logger;

    public PostsController(AppDbContext context, ILogger<PostsController> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Post>> Post([FromQuery] int galleryId)
    {
        logger.LogInformation($"Creating post belonging to gallery {galleryId}");
        Post post = new("bruh", "bruh", galleryId);
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added post belonging to gallery {galleryId}");
        return Ok(post);
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts()
    {
        logger.LogInformation("Retrieving all posts");
        IList<Post> posts = await context.Posts.ToListAsync();
        logger.LogInformation($"Retrieved {posts.Count} posts");
        return Ok(posts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPost(int id)
    {
        logger.LogInformation($"Retrieving post {id}");
        Post post  = await context.Posts
            .Include(p => p.Upload)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post is null)
        {
            logger.LogInformation($"No post found with id {id}");
            return NotFound();
        }
        logger.LogInformation($"Retrieved post {id}");
        return Ok(post);
    }
    
    
}