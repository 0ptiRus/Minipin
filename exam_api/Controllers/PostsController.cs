using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Helpers;
using ElectronNET.API.Entities;
using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace exam_api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly FileService service;
    private readonly MinioService minio;
    private readonly ILogger logger;
    private readonly RedisService redis_service;

    private readonly int page_size = 5;
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

    [HttpPost("save")]
    public async Task<IActionResult> SavePost([FromBody] SavePostModel model)
    {
        Post? post = await context.Posts.FindAsync(model.PostId);
        Gallery? gallery = await context.Galleries.FindAsync(model.GalleryId);
    
        if (post == null || gallery == null)
            return NotFound();

        // bool isAlreadySaved = await context.SavedPosts.AnyAsync(sp => sp.PostId == model.PostId && sp.GalleryId == model.GalleryId);
        // if (isAlreadySaved)
        //     return BadRequest("Post already saved in this gallery.");

        context.SavedPosts.Add(new SavedPost
        {
            PostId = model.PostId,
            GalleryId = model.GalleryId
        });
        
        await redis_service.RemoveAllKeysAsync(cache_prefix);

        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<Post>> Post([FromForm] CreatePostModel model)
    {
        logger.LogInformation($"Creating post belonging to gallery {model.GalleryId}");
        Post post = new(model.Name, model.Description, model.GalleryId, model.UserId);
        
        
        // Parse and attach tags
        if (!string.IsNullOrWhiteSpace(model.Tags))
        {
            IEnumerable<string> tag_names = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Distinct();

            List<Tag> existing_tags = await context.Tags
                .Where(t => tag_names.Contains(t.Name))
                .ToListAsync();

            post.Tags = new List<Tag>();
            foreach (string tag_name in tag_names)
            {
                var tag = existing_tags.FirstOrDefault(t => t.Name == tag_name);
                if (tag == null)
                {
                    tag = new Tag { Name = tag_name };
                    context.Tags.Add(tag);
                }
                post.Tags.Add(tag);
            }
        }
        
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
    
    [HttpGet("search")]
    public async Task<IActionResult> SearchPosts([FromQuery] string query, [FromQuery] int page = 1)
    {
        if (page < 1)
            page = 1;

        int page_size = 10;
        int skip = (page - 1) * page_size;

        // Step 1: Search posts based on query matching name, description, or user name
        List<Post> search_results = await context.Posts
            .Include(p => p.Upload)
            .Include(p => p.User)
            .ThenInclude(u => u.Pfp)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .ThenInclude(u => u.Pfp)
            .Include(p => p.Tags)
            .Where(p => !p.IsDeleted && 
                        (p.Name.Contains(query) 
                         || p.Description.Contains(query) 
                         || p.User.UserName.Contains(query)
                         || p.Tags.Any(t => t.Name.Contains(query))))
            .Skip(skip)
            .Take(page_size)
            .ToListAsync();

        
        List<PostModel> post_dtos = new List<PostModel>();

        foreach (Post post in search_results)
        {
            PostModel post_dto = new PostModel
            {
                Id = post.Id,
                Name = post.Name,
                Description = post.Description,
                UserId = post.UserId,
                ImageUrl = post.Upload != null
                    ? await minio.GetFileUrlAsync(post.Upload.ObjectName, minio.GetBucketNameForFile(post.Upload.ContentType))
                    : null
            };

            IList<Comment> comments = post.Comments.Where(c => !c.IsDeleted).ToList();

            foreach (var comment in comments)
            {
                post_dto.Comments.Add(new CommentModel
                {
                    Id = comment.Id,
                    Text = comment.Text,
                    Username = comment.User.UserName,
                    ProfilePictureUrl = comment.User.Pfp != null
                        ? await minio.GetFileUrlAsync(comment.User.Pfp.ObjectName, minio.GetBucketNameForFile(comment.User.Pfp.ContentType))
                        : null
                });
            }

            post_dtos.Add(post_dto);
        }
        
        return Ok(post_dtos);
    }


    [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] string? filter = "", [FromQuery] string? search = "")
    {
            PagedResponse<AdminPostModel> cached_result =
                await redis_service.GetValueAsync<PagedResponse<AdminPostModel>>($"{cache_prefix}:{(filter == "" ? "all" : filter)}" +
                    $":" +
                    $"{(search == "" ? "" : search)}");
            if (cached_result != null)
            {
                logger.LogInformation("Found all users in cache");
                return Ok(cached_result);
            }
        
        logger.LogInformation("Returning all posts");
        IList<Post> posts = await context.Posts
            .Include(p => p.Upload)
            .Include(p => p.User)
            .ToListAsync();

        if (filter != "")
        {
            posts = filter.ToLower() switch
            {
                "flagged" => posts
                                .Where(post => context.Reports
                                    .Any(report => report.ReportedItemId == post.Id && report.ReportedItemType == "Post"))
                                .ToList(),
                "deleted" => posts.Where(p => p.IsDeleted).ToList(),
                _ => posts
            };
            logger.LogInformation($"Applied filter {filter}");
        }

        if (search != "")
        {
            posts = posts.Where(p => p.User.UserName.ToLower().Contains(search.ToLower()) 
                                     || 
                                     p.Name.ToLower().Contains(search.ToLower())
                                     ||
                                     p.Description.ToLower().Contains(search.ToLower())
                                     ).ToList();
        }
        
        IList<AdminPostModel> models = await Task.WhenAll(posts
            .Select(async p => new AdminPostModel()
            {
                Id = p.Id,
                GalleryId = p.GalleryId,
                Name = p.Name,
                Username = p.User.UserName,
                UserId = p.User.Id,
                Description = p.Description,
                ImageUrl = await minio.GetFileUrlAsync(p.Upload.ObjectName, minio.GetBucketNameForFile(p.Upload.ContentType)),
                IsDeleted = p.IsDeleted,
                IsFlagged = posts.Any(p => context.Reports.Any(r => r.ReportedItemId == p.Id && r.ReportedItemType == "Post")),
                
            })
            .ToList());

        PagedResponse<AdminPostModel> response = new PagedResponse<AdminPostModel>
        {
            Items = models.ToList(),
            TotalItems = posts.Count
        };

        redis_service.RemoveCacheAsync($"{cache_prefix}:stats");
        redis_service.SetValueAsync($"{cache_prefix}:{(filter == "" ? "all" : filter)}:{(search == "" ? "" : search)}", response);
        
        return Ok(response);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        GeneralStatsModel cached_stats = await redis_service.GetValueAsync<GeneralStatsModel>($"{cache_prefix}:stats");
        if (cached_stats != null)
        {
            return Ok(cached_stats);
        }
        
        int total = context.Posts.Count();
        int flagged = context.Posts.Count(p => context.Reports.Any(r => r.ReportedItemId == p.Id && r.ReportedItemType == "Post"));
        int deleted = context.Posts.Count(p => p.IsDeleted);

        GeneralStatsModel stats = new GeneralStatsModel()
        {
            Total = total,
            Flagged = flagged,
            Deleted = deleted
        };
        
        await redis_service.SetValueAsync($"{cache_prefix}:stats", stats);

        return Ok(stats);
    }
        
        
    [HttpGet("all")]
    public async Task<IActionResult> GetPostsByPage([FromQuery] int galleryId, [FromQuery] int page = 1)
    {
        // IList<PostModel> cached_posts = await redis_service.GetValueAsync<List<PostModel>>($"{cache_prefix}:{galleryId}:{page}");
        // if(cached_posts is not null)
        //     return Ok(cached_posts);
        
        if (page < 1)
            page = 1;

        var skip = (page - 1) * page_size;

        List<Post> posts = await context.Posts
            .Include(p => p.Upload)
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                    .ThenInclude(u => u.Pfp)
            .Where(p => p.GalleryId == galleryId) 
            .Skip(skip)
            .Take(page_size)
            .ToListAsync();
        
        List<Post> saved_posts = await context.SavedPosts
            .Where(p => p.GalleryId == galleryId)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.Comments)
                    .ThenInclude(c => c.User)
                        .ThenInclude(u => u.Pfp)
            .Include(sp => sp.Post.Upload)
            .Include(sp => sp.Post.User)
                .ThenInclude(u => u.Pfp)
            .Select(sp => sp.Post)
            .ToListAsync();
        
        posts.AddRange(saved_posts);

        var post_dtos = new List<PostModel>();

        foreach (var post in posts)
        {
            var postDto = new PostModel
            {
                Id = post.Id,
                Name = post.Name,
                Description = post.Description,
                UserId = post.UserId,
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
    
    [HttpGet("feed")]
    public async Task<IActionResult> GetUserFeed([FromQuery] string userId, [FromQuery] int page = 1)
    {
        if (page < 1)
            page = 1;

        int pageSize = 10;
        int skip = (page - 1) * pageSize;

        // Step 1: Get tag names used by user's posts
        var user_tags = await context.Posts
            .Where(p => p.UserId == userId)
            .SelectMany(p => p.Tags.Select(t => t.Name))
            .Distinct()
            .ToListAsync();
        
        if (user_tags is null || user_tags.Count == 0)
        {
            user_tags = new List<string> { "art", "nature", "travel", "design", "inspiration", "meme", "funny" };
        }
        
        List<Post> tagged_posts = await context.Posts
            .Where(p => !p.IsDeleted && p.Tags.Any(t => user_tags.Contains(t.Name)))
            .Include(p => p.Upload)
            .Include(p => p.User)
                .ThenInclude(u => u.Pfp)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                    .ThenInclude(u => u.Pfp)
            .Include(p => p.Tags) 
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        
        List<PostModel> post_dtos = new List<PostModel>();

        foreach (Post post in tagged_posts)
        {
            PostModel post_dto = new PostModel
            {
                Id = post.Id,
                Name = post.Name,
                Description = post.Description,
                UserId = post.UserId,
                ImageUrl = post.Upload != null
                    ? await minio.GetFileUrlAsync(post.Upload.ObjectName, minio.GetBucketNameForFile(post.Upload.ContentType))
                    : null
            };

            IList<Comment> comments = post.Comments.Where(c => !c.IsDeleted).ToList();
            
            foreach (var comment in comments)
            {
                post_dto.Comments.Add(new CommentModel
                {
                    Id = comment.Id,
                    Text = comment.Text,
                    Username = comment.User.UserName,
                    ProfilePictureUrl = comment.User.Pfp != null
                        ? await minio.GetFileUrlAsync(comment.User.Pfp.ObjectName, minio.GetBucketNameForFile(comment.User.Pfp.ContentType))
                        : null
                });
            }

            post_dtos.Add(post_dto);
        }

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

        var saved_posts = await context.SavedPosts
            .Where(p => p.PostId == id)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.Comments)
                    .ThenInclude(c => c.User)
                        .ThenInclude(u => u.Pfp)
            .Include(sp => sp.Post.Upload)
            .Include(sp => sp.Post.User)
                .ThenInclude(u => u.Pfp)
            .Select(sp => sp.Post)
            .ToListAsync();
        
        postEntity.AddRange(saved_posts);

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
        
        List<CommentModel> model_comments = flat_comments.Where(c => !c.IsDeleted)
            .Select(c => new CommentModel
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
            UserId = post.UserId,
            ImageUrl    = await minio.GetFileUrlAsync(post.Upload.ObjectName, minio.GetBucketNameForFile(post.Upload.ContentType)),
            Comments    = top_level
        };

        return Ok(postModel);
    }

    [HttpPost("edit")]
    public async Task<IActionResult> EditPost(EditPostModel model)
    {
        Post post = await context.Posts.FindAsync(model.Id);
        
        if(post is null)
            return BadRequest("No such post exists!");
        
        post.Name = model.Name;
        post.Description = model.Description;

        context.Update(post);
        await context.SaveChangesAsync();

        await redis_service.RemoveAllKeysAsync(cache_prefix);

        return Ok();
    }

    [HttpPost("move")]
    public async Task<IActionResult> MovePost(PostUpdateModel model)
    {
        Post post = await context.Posts.FindAsync(model.PostId);
        Gallery gallery = await context.Galleries.FindAsync(model.GalleryId);
        if (post is null || gallery is null)
            return BadRequest("No such post exists!");

        post.GalleryId = model.GalleryId;
        context.Update(post);
        await context.SaveChangesAsync();

        await redis_service.RemoveAllKeysAsync(cache_prefix);

        return Ok();
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemovePost(PostUpdateModel model)
    {
        Post post = await context.Posts.FindAsync(model.PostId);
        Gallery gallery = await context.Galleries.FindAsync(model.GalleryId);
        if (post is null || gallery is null)
            return BadRequest("No such post or gallery exists!");

        post.IsDeleted = true;
        context.Update(post);
        await context.SaveChangesAsync();

        await redis_service.RemoveAllKeysAsync(cache_prefix);
        
        return Ok();
    }

    [HttpPost("restore/{post_id:int}")]
    public async Task<IActionResult> RestorePost(int post_id)
    {
        Post post = await context.Posts.FindAsync(post_id);
        if(post is null)
            return BadRequest("No such post exists!");

        post.IsDeleted = false;
        context.Update(post);
        await context.SaveChangesAsync();
        
        await redis_service.RemoveAllKeysAsync(cache_prefix);

        return Ok();
    }

    
    
}