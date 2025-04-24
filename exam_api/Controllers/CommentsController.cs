using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly ILogger logger;
    private readonly RedisService redis_service;
    private readonly MinioService minio_service;
    private readonly string cache_prefix = "Comments";

    public CommentsController(AppDbContext context, ILogger<CommentsController> logger, RedisService redis_service, MinioService minio_service)
    {
        this.context = context;
        this.logger = logger;
        this.redis_service = redis_service;
        this.minio_service = minio_service;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery] string? filter = "", [FromQuery] string? search = "")
    {
            PagedResponse<AdminCommentModel> cached_result =
                await redis_service.GetValueAsync<PagedResponse<AdminCommentModel>>($"{cache_prefix}:{(filter == "" ? "all" : filter)}" +
                    $":" +
                    $"{(search == "" ? "" : search)}");
            if (cached_result != null)
            {
                logger.LogInformation("Found all comments in cache");
                return Ok(cached_result);
            }
        
        logger.LogInformation("Returning all comments");
        IList<Comment> comments = await context.Comments
            .Include(c => c.Post)
                .ThenInclude(p => p.Upload)
            .Include(c => c.User)
            .ToListAsync();

        if (filter != "")
        {
            comments = filter.ToLower() switch
            {
                "flagged" => comments
                                .Where(post => context.Reports
                                    .Any(report => report.ReportedItemId == post.Id && report.ReportedItemType == "Comment"))
                                .ToList(),
                "deleted" => comments.Where(p => p.IsDeleted).ToList(),
                _ => comments
            };
            logger.LogInformation($"Applied filter {filter}");
        }

        if (search != "")
        {
            comments = comments.Where(c => c.User.UserName.ToLower().Contains(search.ToLower()) 
                                     || 
                                     c.Text.ToLower().Contains(search.ToLower())).ToList();
        }
        
        IList<AdminCommentModel> models = await Task.WhenAll(comments
            .Select(async c => new AdminCommentModel()
            {
                Id = c.Id,
                Text = c.Text,
                Username = c.User.UserName,
                CommentPostName = c.Post.Name,
                CommentPostImage = await minio_service.GetFileUrlAsync(c.Post.Upload.ObjectName, minio_service.GetBucketNameForFile(c.Post.Upload.ContentType)),
                IsDeleted = c.IsDeleted,
                IsFlagged = comments.Any(c => context.Reports.Any(r => r.ReportedItemId == c.Id && r.ReportedItemType == "Comment")),
                
            })
            .ToList());

        PagedResponse<AdminCommentModel> response = new PagedResponse<AdminCommentModel>
        {
            Items = models.ToList(),
            TotalItems = comments.Count
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
        
        int total = context.Comments.Count();
        int flagged = context.Comments.Count(p => context.Reports.Any(r => r.ReportedItemId == p.Id && r.ReportedItemType == "Comment"));
        int deleted = context.Comments.Count(p => p.IsDeleted);

        GeneralStatsModel stats = new GeneralStatsModel()
        {
            Total = total,
            Flagged = flagged,
            Deleted = deleted
        };
        
        await redis_service.SetValueAsync($"{cache_prefix}:stats", stats);

        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetComment(int id)
    {
        Comment comment = await context.Comments.FindAsync(id);

        if (comment is not null)
        {
            logger.LogInformation($"Returning {comment.Id} comment");
            return Ok(comment);
        }
        logger.LogInformation($"Comment with {id} was not found, returning 404");
        return NotFound();
    }

    // [HttpPost]
    // public async Task<ActionResult<Comment>> CreateComment(Comment comment)
    // {
    //     context.Comments.Add(comment);
    //     await context.SaveChangesAsync();
    //
    //     return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    // }

    [HttpPost("update/")]
    public async Task<IActionResult> UpdateComment(int id, Comment comment)
    {
        if (id != comment.Id)
        {
            logger.LogInformation($"Id {id} is not the same as id {comment.Id}");
            return BadRequest();   
        }

        context.Comments.Update(comment);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CommentExists(id))
            {
                logger.LogError("A db error has occured while updating the comment");
                return Problem();   
            }
            else
                throw;
        }

        logger.LogInformation($"Returning comment with id {id}");
        return Ok(comment);
    }

    [HttpPost("comment")]
    public async Task<IActionResult> PostComment([FromBody] CommentModel comment_model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        Post post = await context.Posts.FirstOrDefaultAsync(i => i.Id == comment_model.PostId);
        if (post == null)
        {
            logger.LogInformation($"Post with id {comment_model.PostId} was not found");
        }

        Comment comment = new Comment
        {
            PostId = comment_model.PostId,
            Text = comment_model.Text,
            UserId = comment_model.UserId,
            CreatedAt = comment_model.CreatedAt
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        await redis_service.RemoveAllKeysAsync(cache_prefix);

        logger.LogInformation($"Returning comment with id {comment.Id}");
        return Ok(comment_model);
    }
    
    [HttpPost("reply")]
    public async Task<IActionResult> PostReply([FromBody] CommentModel comment_model)
    {
        Post post = await context.Posts.FirstOrDefaultAsync(i => i.Id == comment_model.PostId);
        if (post == null)
        {
            logger.LogInformation($"Post with id {comment_model.PostId} was not found");
        }

        Comment comment = new Comment
        {
            PostId = comment_model.PostId,
            Text = comment_model.Text,
            UserId = comment_model.UserId,
            CreatedAt = comment_model.CreatedAt,
            ParentCommentId = comment_model.ParentCommentId
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();
        
        await redis_service.RemoveAllKeysAsync(cache_prefix);

        logger.LogInformation($"Returning comment with id {comment.Id}");
        return Ok(comment_model);
    }

    [HttpPost("remove")]
    public async Task<IActionResult> DeleteComment([FromBody] int CommentId)
    {
        Comment comment = await context.Comments.FindAsync(CommentId);
        if (comment == null)
        {
            logger.LogInformation($"Comment with id {comment} was not found");
        }

        comment.IsDeleted = true;
        await context.SaveChangesAsync();

        logger.LogInformation($"Deleted comment with id {CommentId}");
        return Ok();
    }
    
    [HttpPost("restore")]
    public async Task<IActionResult> RestoreComment([FromBody] int CommentId)
    {
        Comment comment = await context.Comments.FindAsync(CommentId);
        if (comment == null)
        {
            logger.LogInformation($"Comment with id {comment} was not found");
        }

        comment.IsDeleted = false;
        await context.SaveChangesAsync();

        logger.LogInformation($"Restored comment with id {CommentId}");
        return Ok();
    }

    private bool CommentExists(int id)
    {
        return context.Comments.Any(e => e.Id == id);
    }
}