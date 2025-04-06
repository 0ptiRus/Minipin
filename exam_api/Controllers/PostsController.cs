using CommunityToolkit.HighPerformance.Helpers;
using ElectronNET.API.Entities;
using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly FileService service;
    private readonly ILogger logger;

    public PostsController(AppDbContext context, FileService service, ILogger<PostsController> logger)
    {
        this.context = context;
        this.service = service;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Post>> Post([FromBody] CreatePostModel model)
    {
        logger.LogInformation($"Creating post belonging to gallery {model.GalleryId}");
        Post post = new(model.Name, model.Description, model.GalleryId);
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        string object_name = $"{Guid.NewGuid()}_{model.File.FileName}";
        UploadedFile file = new UploadedFile
        {
            ObjectName = object_name,
            PostId = post.Id,
            GalleryId = null,
            UserId = null,
        };
        
        logger.LogInformation($"Creating file associated with post {post.Id}");
        var result = await service.CreateFile(file, model.File);
        if (result is not null)
        {
            logger.LogInformation($"Added post belonging to gallery {model.GalleryId}");
            return Ok(post);   
        }
        logger.LogError($"Couldn't create a post");
        return StatusCode(500);
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