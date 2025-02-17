using exam_api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class LikesController : ControllerBase
{
    private readonly ApiDbContext context;

    public LikesController(ApiDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikes()
    {
        return await context.Likes.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Like>> GetLike(int id)
    {
        Like like = await context.Likes.FindAsync(id);

        if (like == null)
            return NotFound();

        return like;
    }
    
    [HttpPost]
    public async Task<ActionResult<Like>> LikeImage(int id)
    {
        Image image = await context.Images.FindAsync(id);
        if (image == null)
            return NotFound();

        Like like = new Like
        {
            ImageId = id,
            UserId = User.Identity.Name
        };

        image.Likes.Add(like);
        await context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpDelete]
    public async Task<IActionResult> UnlikeImage(int id)
    {
        Like like = await context.Likes.FirstOrDefaultAsync(l => l.ImageId == id && l.UserId == User.Identity.Name);
        if (like == null)
            return NotFound();

        context.Likes.Remove(like);
        await context.SaveChangesAsync();

        return NoContent();
    }
}