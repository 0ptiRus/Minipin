using System.Collections;
using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class GalleriesController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly MinioService minio;
    private readonly FileService file_service;
    private readonly ILogger<GalleriesController> logger;
    private readonly RedisService redis_service;

    private readonly string cache_prefix = "Galleries";

    public GalleriesController(AppDbContext context, ILogger<GalleriesController> logger, 
        MinioService minio, FileService file_service, RedisService redis_service)
    {
        this.context = context;
        this.logger = logger;
        this.minio = minio;
        this.file_service = file_service;
        this.redis_service = redis_service;
    }

    [HttpGet]
    public async Task<IActionResult> GetGalleries()
    {
        logger.LogInformation("Retrieving public galleries");
        
        var galleries = await context.Galleries
            .Where(g => !g.IsPrivate)
            .ToListAsync();

        logger.LogInformation($"Retrieved {galleries.Count} public galleries");
        return Ok(galleries);
    }

    // [HttpGet("{id:int}")]
    // public async Task<IActionResult> GetGallery(int id)
    // {
    //     logger.LogInformation($"Attempting to retrieve gallery with ID: {id}");
    //     
    //     var gallery = await context.Galleries.FindAsync(id);
    //
    //     if (gallery != null)
    //     {
    //         logger.LogInformation($"Gallery {id} found successfully");
    //         return Ok(gallery);
    //     }
    //
    //     logger.LogWarning($"Gallery {id} not found");
    //     return NotFound();
    // }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserGallery(int id)
    {
        GalleryDetailsModel cached_model =
            await redis_service.GetValueAsync<GalleryDetailsModel>($"{cache_prefix}:{id}");
        if (cached_model != default)
        {
            return Ok(cached_model);
        }
        
        logger.LogInformation($"Retrieving gallery {id}");
        
        var gallery = await context.Galleries
            .Include(g => g.Posts)
                .ThenInclude(p => p.Comments)
            .Include(g => g.Posts)
                .ThenInclude(p => p.Upload)
            .Include(g => g.User)
                .ThenInclude(u => u.Pfp)
            .Include(g => g.Cover)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gallery != null)
        {
            logger.LogInformation($"Gallery {id} with {gallery.Posts?.Count} posts found for user");
            
            string cover_url = await minio.GetFileUrlAsync(gallery.Cover.ObjectName, minio.GetBucketNameForFile(gallery.Cover.ContentType));
            string pfp = await minio.GetFileUrlAsync(gallery.User.Pfp.ObjectName, minio.GetBucketNameForFile(gallery.User.Pfp.ContentType));
            IList<PreviewPostModel> posts = await Task.WhenAll(gallery.Posts
                .Select(async p => new PreviewPostModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = await minio.GetFileUrlAsync(p.Upload.ObjectName, minio.GetBucketNameForFile(p.Upload.ContentType)),
                    CommentsCount = p.Comments.Count,
                }));
            
            GalleryDetailsModel model = new GalleryDetailsModel
            {
                Id = gallery.Id,
                Name = gallery.Name,
                Description = gallery.Description,
                Username = gallery.User.UserName,
                UserId = gallery.User.Id,
                CoverUrl = cover_url,
                Posts = posts,
                Pfp = pfp,
                PostsCount = gallery.Posts.Count,
                CommentsCount = gallery.Posts.Sum(p => p.Comments.Count)
            };
            
            await redis_service.SetValueAsync($"{cache_prefix}:{id}", model);
            
            return Ok(model);
        }

        logger.LogWarning($"Gallery {id} not found");
        return NotFound();
    }

    [HttpGet("images/{id:int}")]
    public async Task<IActionResult> GetGalleryWithImages(int id)
    {
        logger.LogInformation($"Retrieving gallery {id} with images");
        
        var gallery = await context.Galleries
            .Include(g => g.Posts)
                .ThenInclude(p => p.Upload)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gallery == null)
        {
            logger.LogWarning($"Gallery {id} not found");
            return NotFound();
        }

        try
        {
            logger.LogInformation($"Gallery {id} with {gallery.Posts.Count} images retrieved");
            return Ok(gallery);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to load images for gallery {id}");
            return StatusCode(500);
        }
    }

    [HttpGet("{user_id}")]
    public async Task<IList<PreviewGalleryModel>> GetUserGalleries(string user_id)
    {
        logger.LogInformation($"Retrieving galleries for user {user_id}");
        
        var galleries = await context.Galleries
            .Where(g => g.UserId == user_id && !g.IsDeleted)
            .Include(g => g.Posts)
                .ThenInclude(p => p.Upload)
            .Include(g => g.User)
            .ToListAsync();

        IList<PreviewGalleryModel> models = galleries
            .Select(g => new PreviewGalleryModel
            {
                Id = g.Id,
                UserId = g.UserId,
                Name = g.Name,
            })
            .ToList();

        logger.LogInformation($"Retrieved {galleries.Count} galleries for user {user_id}");
        return models;
    }

    [HttpGet("feed/{user_id}")]
    public async Task<IList<Gallery>> GetFeed(string user_id)
    {
        logger.LogInformation($"Retrieving feed for user {user_id}");
        
        IList<Gallery> cached_galleries = await redis_service.GetValueAsync<List<Gallery>>($"{cache_prefix}:feed:{user_id}");
        if(cached_galleries != default)
            return cached_galleries;
        
        var followedUserIds = context.Follows
            .Where(f => f.FollowerId == user_id)
            .Select(f => f.FollowedId)
            .ToList();

        var galleries = await context.Galleries
            .Where(g => followedUserIds.Contains(g.UserId))
            .Where(g => !g.IsPrivate)
            .Include(g => g.Posts)
                .ThenInclude(p => p.Upload)
            .Where(g => g.Posts.Any())
            .ToListAsync();
        
        await redis_service.SetValueAsync($"{cache_prefix}:feed:{user_id}", galleries);

        logger.LogInformation($"Retrieved {galleries.Count} galleries for feed");
        return galleries;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGallery([FromForm] CreateGalleryModel gallery)
    {
        logger.LogInformation($"Creating new gallery for user {gallery.UserId}");
        
        try
        {
            Gallery new_gallery = new(gallery.Name, gallery.UserId, gallery.IsPrivate);
            context.Galleries.Add(new_gallery);
            await context.SaveChangesAsync();

            if (gallery.Image is not null)
            {
                string object_name = $"{Guid.NewGuid()}_{gallery.Image.Name}";
                UploadedFile file = new UploadedFile
                {
                    ObjectName = object_name,
                    ContentType = gallery.Image.ContentType,
                    GalleryId = new_gallery.Id
                };

                if (await file_service.CreateFile(file, gallery.Image, minio.GetBucketNameForFile(file.ContentType)) is null)
                {
                    logger.LogWarning("Failed to create new gallery - failed to add cover image");
                }    
            }
            
            logger.LogInformation($"Gallery {new_gallery.Id} created successfully");
            await redis_service.RemoveAllKeysAsync($"{cache_prefix}:");
            return CreatedAtAction(nameof(CreateGallery), new { id = new_gallery.Id }, new_gallery.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to create gallery for user {gallery.UserId}");
            return StatusCode(500);
        }
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateGallery(Gallery gallery)
    {
        
        try
        {
            context.Entry(gallery).State = EntityState.Modified;
            await context.SaveChangesAsync();

            logger.LogInformation($"Gallery {gallery.Id} updated successfully");
            return Ok(gallery);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, $"Concurrency error updating gallery {gallery.Id}");
            if (!GalleryExists(gallery.Id))
            {
                logger.LogWarning($"Gallery {gallery.Id} does not exist");
                return NotFound();
            }
            else
            {
                throw;
            }
        }
    }

    [HttpPost("edit")]
    public async Task<IActionResult> EditGallery(EditGalleryModel model)
    {
        Gallery? gallery = await context.Galleries.FindAsync(model.Id);

        if (gallery is null)
            return BadRequest("No such gallery!");

        gallery.Name = model.Name;
        gallery.Description = model.Description;

        context.Update(gallery);
        await context.SaveChangesAsync();

        await redis_service.RemoveCacheAsync($"{cache_prefix}");

        return Ok();
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteGallery(RemoveGalleryModel model)
    {
        logger.LogInformation($"Deleting gallery {model.Id}");
        Gallery gallery = await context.Galleries
            .Include(g => g.Posts)
            .FirstOrDefaultAsync(g => g.Id == model.Id);

        if (gallery == null)
        {
            logger.LogWarning($"Gallery {model.Id} not found");
            return NotFound();
        }

        try
        {
            gallery.IsDeleted = true;
            foreach (Post post in gallery.Posts)
                post.IsDeleted = true;
            
            await context.SaveChangesAsync();

            logger.LogInformation($"Gallery {model.Id} deleted successfully");
            await redis_service.RemoveCacheAsync($"{cache_prefix}");
            return Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to delete gallery {model.Id}");
            return StatusCode(500);
        }
    }

    private bool GalleryExists(int id)
    {
        logger.LogDebug($"Checking existence of gallery with ID: {id}");
        return context.Galleries.Any(e => e.Id == id);
    }
}
}
