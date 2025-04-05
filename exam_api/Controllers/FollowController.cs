using exam_api.Entities;
using exam_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace exam_api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]    
    public class FollowsController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly ILogger<FollowsController> logger;

    public FollowsController(AppDbContext context, ILogger<FollowsController> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    [HttpGet("followers/{user_id}")]
    public async Task<IActionResult> GetFollowers(string user_id)
    {
        logger.LogInformation($"Retrieving followers for user: {user_id}");
        
        List<UserModel> followers = await context.Follows
            .Include(f => f.Followed)
            .Where(f => f.FollowedId == user_id)
            .Select(f => new UserModel(f.FollowerId, f.Follower.UserName))
            .ToListAsync();

        logger.LogInformation($"{followers.Count} followers retrieved for user {user_id}");
        return Ok(followers);
    }

    [HttpGet("followed/{user_id}")]
    public async Task<IActionResult> GetFollowed(string user_id)
    {
        logger.LogInformation($"Retrieving followed users for user: {user_id}");

        List<UserModel> followed = await context.Follows
            .Include(f => f.Follower)
            .Where(f => f.FollowerId == user_id)
            .Select(f => new UserModel(f.FollowedId, f.Followed.UserName))
            .ToListAsync();

        logger.LogInformation($"{followed.Count} followed users retrieved for user {user_id}");
        return Ok(followed);
    }

    [HttpGet("follower/{id:int}")]
    public async Task<IActionResult> GetFollower(int id)
    {
        logger.LogDebug($"Attempting to retrieve follow record with ID: {id}");
        
        var follow = await context.Follows.FindAsync(id);
        
        if (follow != null)
        {
            logger.LogInformation($"Follow record {id} found successfully");
            return Ok(follow);
        }
        
        logger.LogWarning($"Follow record {id} not found");
        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> PostFollower(Follow follow)
    {
        logger.LogInformation($"Creating new follow relationship: Follower {follow.FollowerId} → Followed {follow.FollowedId}");
        
        try
        {
            context.Follows.Add(follow);
            await context.SaveChangesAsync();
            
            logger.LogInformation($"Follow relationship created successfully: ID {follow.Id}");
            return CreatedAtAction(nameof(GetFollower), new { id = follow.Id }, follow);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to create follow relationship: Follower {follow} → Followed {follow.Followed}");
            return StatusCode(500);
        }
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteFollower(Follow follow)
    {
        logger.LogInformation($"Deleting follow relationship: Follower {follow.FollowerId} → Followed {follow.FollowedId}");

        if (follow == null)
        {
            logger.LogWarning($"Follow relationship not found: Follower {follow.FollowerId} → Followed {follow.FollowedId}");
            return Problem(); // Or return NotFound()
        }

        context.Follows.Remove(follow);
        await context.SaveChangesAsync();
        
        logger.LogInformation($"Follow relationship deleted successfully: ID {follow.Id}");
        return Ok();
    }

    [HttpPost("update")]
    public async Task<IActionResult> PutFollower(Follow follow)
    {
        logger.LogInformation($"Updating follow record with ID: {follow.Id}");

        try
        {
            context.Entry(follow).State = EntityState.Modified;
            await context.SaveChangesAsync();
            
            logger.LogInformation($"Follow record {follow.Id} updated successfully");
            return Ok();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, $"Concurrency error updating follow record {follow.Id}");
            if (!FollowExists(follow.Id))
            {
                logger.LogWarning($"Follow record {follow.Id} does not exist");
                return NotFound();
            }
            else
            {
                throw;
            }
        }
    }

    private bool FollowExists(int id)
    {
        logger.LogDebug($"Checking existence of follow record with ID: {id}");
        return context.Follows.Any(e => e.Id == id);
    }
}
}
