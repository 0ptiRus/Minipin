using exam_api.Entities;
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

    [HttpPost]
    public async Task<IActionResult> PostComment([FromQuery] int id,[FromQuery] string user_id,[FromQuery] string text)
    {
        UploadedFile uploadedFile = await context.Files.FirstOrDefaultAsync(i => i.Id == id);
        if (uploadedFile == null)
        {
            logger.LogInformation($"Image with id {id} was not found");
        }

        Comment comment = new Comment
        {
            PostId = id,
            Text = text,
            UserId = user_id
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        logger.LogInformation($"Returning comment with id {id}");
        return Ok(comment);
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