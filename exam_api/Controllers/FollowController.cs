using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Exception = System.Exception;

namespace exam_api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]    
    public class FollowsController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly ILogger<FollowsController> logger;
    private readonly RedisService redis;
    private readonly MinioService minio;

    public FollowsController(AppDbContext context, ILogger<FollowsController> logger, RedisService redis, MinioService minio)
    {
        this.context = context;
        this.logger = logger;
        this.redis = redis;
        this.minio = minio;
    }

    [HttpGet("follow_list")]
    public async Task<IActionResult> GetFollowList([FromQuery] string user_id, [FromQuery] string viewing_user_id)
    {
        ApplicationUser? target_user = await context.Users
            .Include(u => u.Followers)
            .Include(u => u.Followed)
            .Include(u => u.Pfp)
            .FirstOrDefaultAsync(u => u.Id == user_id);

        if (target_user == null)
            return NotFound();
        
        ApplicationUser? current_user = await context.Users
            .Include(u => u.Followed)
            .FirstOrDefaultAsync(u => u.UserName == viewing_user_id);
        
        IList<Follow> followers = await context.Follows
            .Where(f => f.FollowedId == target_user.Id)
            .Include(f => f.Follower)
                .ThenInclude(f => f.Followers)
            .Include(f => f.Follower)
                .ThenInclude(f => f.Pfp)
            .ToListAsync();
        
        IList<Follow> following = await context.Follows
            .Where(f => f.FollowerId == target_user.Id)
            .Include(f => f.Followed)
                .ThenInclude(f => f.Followers)
            .Include(f => f.Followed)
                .ThenInclude(f => f.Pfp)
            .ToListAsync();

        // UserListModel profile_data = new UserListModel
        // {
        //     Username = target_user.UserName,
        //     FollowerCount = followers.Count,
        //     FollowingCount = following.Count,
        //     Followers = followers.Select(f => new UserInListModel
        //     {
        //         Id = f.Follower.Id,
        //         Username = f.Follower.UserName,
        //         PfpUrl = await minio.GetFileUrlAsync(f.Follower.Pfp.ObjectName, minio.GetBucketNameForFile(f.Follower.Pfp.ObjectName)),
        //         FollowerCount = f.Follower.Followers.Count(),
        //         IsFollowing = current_user?.Followed.Any(x => x.FollowedId == f.Follower.Id) ?? false
        //     }).ToList(),
        //     Following = following.Select(f => new UserInListModel
        //     {
        //         Id = f.Followed.Id,
        //         Username = f.Followed.UserName,
        //         PfpUrl = await minio.GetFileUrlAsync(f.Followed.Pfp.ObjectName, minio.GetBucketNameForFile(f.Followed.Pfp.ObjectName)),
        //         FollowerCount = f.Follower.Followers.Count(),
        //         IsFollowing = current_user?.Followed.Any(x => x.FollowedId == f.Followed.Id) ?? false
        //     }).ToList()
        // };
        
        // Create the profile data object
        UserListModel profile_data = new UserListModel
        {
            Username = target_user.UserName,
            PfpUrl = await minio.GetFileUrlAsync(target_user.Pfp.ObjectName, minio.GetBucketNameForFile(target_user.Pfp.ContentType)),
            FollowerCount = followers.Count,
            FollowingCount = following.Count,
            Followers = new List<UserInListModel>(),
            Following = new List<UserInListModel>()
        };

        // For each follower, fetch the profile picture URL asynchronously
        foreach (Follow follower in followers)
        {
            string pfp_url = await minio.GetFileUrlAsync(follower.Follower.Pfp.ObjectName, minio.GetBucketNameForFile(follower.Follower.Pfp.ContentType));

            profile_data.Followers.Add(new UserInListModel
            {
                Id = follower.Follower.Id,
                Username = follower.Follower.UserName,
                PfpUrl = pfp_url,
                FollowerCount = follower.Follower.Followers.Count(),
                IsFollowing = current_user?.Followed.Any(x => x.FollowedId == follower.Follower.Id) ?? false
            });
        }
        
        foreach (Follow followed in following)
        {
            string pfp_url = await minio.GetFileUrlAsync(followed.Followed.Pfp.ObjectName, minio.GetBucketNameForFile(followed.Followed.Pfp.ContentType));

            profile_data.Following.Add(new UserInListModel
            {
                Id = followed.Followed.Id,
                Username = followed.Followed.UserName,
                PfpUrl = pfp_url,
                FollowerCount = followed.Followed.Followers.Count(),
                IsFollowing = current_user?.Followed.Any(x => x.FollowedId == followed.Followed.Id) ?? false
            });
        }

        return Ok(profile_data);
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

    [HttpPost("follow")]
    public async Task<IActionResult> PostFollower(string user_id, string followed_user_id)
    {
        logger.LogInformation($"Creating new follow relationship: Follower {user_id} → Followed {followed_user_id}");
        
        try
        {
            Follow follow = new Follow(user_id, followed_user_id);
            context.Follows.Add(follow);
            await context.SaveChangesAsync();
            
            logger.LogInformation($"Follow relationship created successfully: ID {follow.Id}");
            return CreatedAtAction(nameof(GetFollower), new { id = follow.Id }, follow);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to create follow relationship: Follower {user_id} → Followed {followed_user_id}");
            return StatusCode(500);
        }
    }

    [HttpPost("unfollow")]
    public async Task<IActionResult> Unfollow(string user_id, string followed_user_id)
    {
        logger.LogInformation($"User {user_id} trying to unfollow {followed_user_id}");
        
        try
        {
            Follow follow = await context.Follows.Where(f => f.FollowerId == user_id && f.FollowedId == followed_user_id)
                .FirstOrDefaultAsync();
            if (follow == null)
                return NotFound();

            context.Follows.Remove(follow);
            await context.SaveChangesAsync();
            
            logger.LogInformation($"User {user_id} successfully unfollowed {followed_user_id}");
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to create follow relationship: Follower {user_id} → Followed {followed_user_id}: {ex.Message}");
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
