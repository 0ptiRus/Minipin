using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using exam_api.Data;
using Microsoft.AspNetCore.Authorization; // Adjust the namespace as needed

namespace exam_api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly ApiDbContext context;

        public FollowController(ApiDbContext context)
        {
            this.context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Follow>>> GetFollowers()
        {
            return await context.Follows.ToListAsync();
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Follow>> GetFollower(int id)
        {
            var follower = await context.Follows.FindAsync(id);

            if (follower == null)
            {
                return NotFound();
            }

            return follower;
        }
        
        [HttpPost]
        public async Task<ActionResult<Follow>> PostFollower(Follow follow)
        {
            context.Follows.Add(follow);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFollower), new { id = follow.Id }, follow);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFollower(int id)
        {
            Follow? follower = await context.Follows.FindAsync(id);
            if (follower == null)
            {
                return NotFound();
            }

            context.Follows.Remove(follower);
            await context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFollower(int id, Follow follow)
        {
            if (id != follow.Id)
            {
                return BadRequest();
            }

            context.Entry(follow).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FollowExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool FollowExists(int id)
        {
            return context.Follows.Any(e => e.Id == id);
        }
    }
}