using exam_api.Entities;
using exam_api.Models;
using exam_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly ILogger logger;
    private readonly RedisService redis_service;

    public CommentsController(AppDbContext context, ILogger<CommentsController> logger, RedisService redis_service)
    {
        this.context = context;
        this.logger = logger;
        this.redis_service = redis_service;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments()
    {
        List<Comment> comments = await context.Comments.ToListAsync();
        logger.LogInformation($"Returning {comments.Count} comments");
        return Ok(comments);
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

        logger.LogInformation($"Returning comment with id {comment.Id}");
        return Ok(comment_model);
    }

    [HttpPost("delete/")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        Comment comment = await context.Comments.FindAsync(id);
        if (comment == null)
        {
            logger.LogInformation($"Comment with id {id} was not found");
        }

        context.Comments.Remove(comment);
        await context.SaveChangesAsync();

        logger.LogInformation($"Deleted comment with id {id}");
        return Ok();
    }

    private bool CommentExists(int id)
    {
        return context.Comments.Any(e => e.Id == id);
    }
}