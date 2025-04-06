using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class GalleriesController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly MinioService minio;
    private readonly ILogger<GalleriesController> logger;

    public GalleriesController(AppDbContext context, ILogger<GalleriesController> logger, MinioService minio)
    {
        this.context = context;
        this.logger = logger;
        this.minio = minio;
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

    [HttpGet()]
    public async Task<IActionResult> GetGallery(int id)
    {
        logger.LogInformation($"Attempting to retrieve gallery with ID: {id}");
        
        var gallery = await context.Galleries.FindAsync(id);

        if (gallery != null)
        {
            logger.LogInformation($"Gallery {id} found successfully");
            return Ok(gallery);
        }

        logger.LogWarning($"Gallery {id} not found");
        return NotFound();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserGallery(int id)
    {
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
            
            string cover_url = await minio.GetFileUrlAsync(gallery.Cover.ObjectName);
            string pfp = await minio.GetFileUrlAsync(gallery.User.Pfp.ObjectName);
            IList<string> post_urls = new List<string>();
            foreach (var post in gallery.Posts)
            {
                string url = await minio.GetFileUrlAsync(post.Upload.ObjectName);
                post_urls.Add(url);
            }
            
            return Ok(new GalleryDetailsModel
            {
                CoverUrl = cover_url,
                Gallery = gallery,
                ImageUrls = post_urls,
                Pfp = pfp
            });
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
    public async Task<IList<Gallery>> GetUserGalleries(string user_id)
    {
        logger.LogInformation($"Retrieving galleries for user {user_id}");
        
        var galleries = await context.Galleries
            .Where(g => g.UserId == user_id)
            .Include(g => g.Posts)
                .ThenInclude(p => p.Upload)
            .Include(g => g.User)
            .ToListAsync();

        logger.LogInformation($"Retrieved {galleries.Count} galleries for user {user_id}");
        return galleries;
    }

    [HttpGet("feed/{user_id}")]
    public async Task<IList<Gallery>> GetFeed(string user_id)
    {
        logger.LogInformation($"Retrieving feed for user {user_id}");
        
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

        logger.LogInformation($"Retrieved {galleries.Count} galleries for feed");
        return galleries;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGallery([FromBody] CreateGalleryModel gallery)
    {
        logger.LogInformation($"Creating new gallery for user {gallery.UserId}");
        
        try
        {
            Gallery new_gallery = new(gallery.Name, gallery.UserId, gallery.IsPrivate);
            context.Galleries.Add(new_gallery);
            await context.SaveChangesAsync();

            logger.LogInformation($"Gallery {new_gallery.Id} created successfully");
            return CreatedAtAction(nameof(GetGallery), new { id = new_gallery.Id }, gallery);
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

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteGallery(Gallery gallery)
    {
        logger.LogInformation($"Deleting gallery {gallery.Id}");
        

        if (gallery == null)
        {
            logger.LogWarning($"Gallery {gallery.Id} not found");
            return NotFound();
        }

        try
        {
            context.Galleries.Remove(gallery);
            await context.SaveChangesAsync();

            logger.LogInformation($"Gallery {gallery.Id} deleted successfully");
            return Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to delete gallery {gallery.Id}");
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
