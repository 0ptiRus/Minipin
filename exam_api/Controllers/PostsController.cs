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
    private readonly MinioService minio;
    private readonly ILogger logger;
    private readonly RedisService redis_service;

    private readonly int page_size = 6;
    private readonly string cache_prefix = "Posts";

    public PostsController(AppDbContext context, FileService service, 
        ILogger<PostsController> logger, MinioService minio, RedisService redis_service)
    {
        this.context = context;
        this.service = service;
        this.logger = logger;
        this.minio = minio;
        this.redis_service = redis_service;
    }

    [HttpPost]
    public async Task<ActionResult<Post>> Post([FromForm] CreatePostModel model)
    {
        logger.LogInformation($"Creating post belonging to gallery {model.GalleryId}");
        Post post = new(model.Name, model.Description, model.GalleryId, model.UserId);
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        
        string object_name = $"{Guid.NewGuid()}_{model.File.FileName}";
        UploadedFile file = new UploadedFile
        {
            ObjectName = object_name,
            ContentType = model.File.ContentType,
            PostId = post.Id,
            GalleryId = null,
            UserId = null,
        };
        
        logger.LogInformation($"Creating file associated with post {post.Id}");
        var result = await service.CreateFile(file, model.File, minio.GetBucketNameForFile(file.ContentType));
        if (result is not null)
        {
            logger.LogInformation($"Added post belonging to gallery {model.GalleryId}");
            await redis_service.RemoveAllKeysAsync($"{cache_prefix}:");
            return Ok(post.Id);   
        }
        logger.LogError($"Couldn't create a post");
        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetPosts()
    {
        logger.LogInformation("Retrieving all posts");
        IList<Post> posts = await context.Posts.ToListAsync();
        logger.LogInformation($"Retrieved {posts.Count} posts");
        return Ok(posts);
    }

    [HttpGet]
    public async Task<IActionResult> GetPostsByPage([FromQuery] int galleryId, [FromQuery] int page = 1)
    {
        // IList<PostModel> cached_posts = await redis_service.GetValueAsync<List<PostModel>>($"{cache_prefix}:{galleryId}:{page}");
        // if(cached_posts is not null)
        //     return Ok(cached_posts);
        
        if (page < 1)
            page = 1;

        var skip = (page - 1) * page_size;

        IList<Post> posts = await context.Posts
            .Include(p => p.Upload)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                    .ThenInclude(u => u.Pfp)
            .Where(p => p.GalleryId == galleryId) 
            .Skip(skip)
            .Take(page_size)
            .ToListAsync();

        var post_dtos = new List<PostModel>();

        foreach (var post in posts)
        {
            var postDto = new PostModel
            {
                Id = post.Id,
                Name = post.Name,
                Description = post.Description,
                ImageUrl = post.Upload != null 
                    ? await minio.GetFileUrlAsync(post.Upload.ObjectName, minio.GetBucketNameForFile(post.Upload.ContentType))
                    : null
            };

            foreach (var comment in post.Comments)
            {
                postDto.Comments.Add(new CommentModel
                {
                    Id = comment.Id,
                    Text = comment.Text,
                    Username = comment.User.UserName,
                    ProfilePictureUrl = comment.User.Pfp != null
                        ? await minio.GetFileUrlAsync(comment.User.Pfp.ObjectName, minio.GetBucketNameForFile(comment.User.Pfp.ContentType))
                        : null
                });
            }

            post_dtos.Add(postDto);
        }
        
        await redis_service.SetValueAsync($"{cache_prefix}:{galleryId}:{page}", post_dtos);

        return Ok(post_dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPost(int id)
    {
        var postEntity = await context.Posts
            .Where(p => p.Id == id)
            .Include(p => p.User).ThenInclude(u => u.Pfp)
            .Include(p => p.Upload)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User).ThenInclude(u => u.Pfp)
            .ToListAsync(); 

        if (!postEntity.Any()) return NotFound();
        
        List<Comment> flat_comments = postEntity.SelectMany(p => p.Comments).ToList();
        
        var distinct_files = flat_comments
            .Select(c => new {
                ObjectName   = c.User.Pfp.ObjectName,
                ContentType  = c.User.Pfp.ContentType
            })
            .Distinct();  

        Dictionary<string, Task<string>> url_tasks = distinct_files.ToDictionary(
            file => file.ObjectName,
            file => minio.GetFileUrlAsync(
                file.ObjectName,
                minio.GetBucketNameForFile(file.ContentType)
            )
        );

        await Task.WhenAll(url_tasks.Values);
        
        List<CommentModel> model_comments = flat_comments.Select(c => new CommentModel
        {
            Id               = c.Id,
            CreatedAt        = c.CreatedAt,
            Username         = c.User.UserName,
            ProfilePictureUrl= url_tasks[c.User.Pfp.ObjectName].Result,
            Text             = c.Text,
            PostId           = c.PostId,
            UserId           = c.UserId,
            ParentCommentId  = c.ParentCommentId,
            Replies          = new List<CommentModel>()
        }).ToList();
        
        Dictionary<int, CommentModel> lookup = model_comments.ToDictionary(c => c.Id);
        foreach (CommentModel cm in model_comments)
        {
            if (cm.ParentCommentId.HasValue && lookup.TryGetValue(cm.ParentCommentId.Value, out var parent))
            {
                parent.Replies.Add(cm);
            }
        }
        
        List<CommentModel> top_level = model_comments
            .Where(c => !c.ParentCommentId.HasValue)
            .ToList();
        
        Post post = postEntity.First();
        PostModel postModel = new PostModel
        {
            Id          = post.Id,
            Name        = post.Name,
            Username    = post.User.UserName,
            Description = post.Description,
            ImageUrl    = await minio.GetFileUrlAsync(post.Upload.ObjectName, minio.GetBucketNameForFile(post.Upload.ContentType)),
            Comments    = top_level
        };

        return Ok(postModel);
    }

    
    
}