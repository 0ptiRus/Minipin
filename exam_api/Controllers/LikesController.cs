using exam_api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LikesController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly ILogger logger;

    public LikesController(AppDbContext context, ILogger<LikesController> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLikes()
    {
        logger.LogInformation("Getting likes");
        List<Like> likes = await context.Likes.ToListAsync();
        logger.LogInformation($"Returned {likes.Count} likes");
        return Ok(likes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLike(int id)
    {
        logger.LogInformation($"Getting like with id {id}");
        Like like = await context.Likes.FindAsync(id);
        if (like is not null)
        {
            logger.LogInformation($"Returned like with id {id}");
            return Ok(like);
        }
        logger.LogWarning($"No like found with id {id}");
        return NotFound();
    }
    
    [HttpPost("like")]
    public async Task<IActionResult> LikePost([FromQuery] int id, [FromQuery] string user_id)
    {
        
        if (await context.Likes
                .SingleOrDefaultAsync(l => l.PostId == id
                                           && l.UserId == user_id) != null)
        {
            if (await UnlikeImage(id, user_id))
            {
                return Ok(new LikeResponse { IsLiked = false, IsUnliked = true});
            }
        }
        
        Post post = await context.Posts
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (post == null)
        {
            logger.LogWarning($"Couldn't find image with id {id} to like");
            return NotFound();   
        }

        Like like = new Like
        {
            PostId = id,
            UserId = user_id
        };

        post.Likes.Add(like);
        await context.SaveChangesAsync();

        logger.LogInformation($"Image with id {id} by user {user_id} has been liked");
        return Ok(new LikeResponse { IsLiked = true, IsUnliked = false});
    }
    
    [HttpPost("unlike")]
    public async Task<bool> UnlikeImage(int id, string user_id)
    {
        Like like = await context.Likes.FirstOrDefaultAsync(l => l.PostId == id && l.UserId == user_id);
        if (like == null)
        {
            logger.LogWarning($"Couldn't find image with id {id} to unlike");
            return false;   
        }

        context.Likes.Remove(like);
        await context.SaveChangesAsync();

        logger.LogInformation($"Image with {id} by user {user_id} has been unliked");
        return true;
    }

    public class LikeResponse
    {
        public bool IsLiked { get; set; }
        public bool IsUnliked { get; set; }
        
    }

}