using exam_api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

    [Authorize(AuthenticationSchemes = nameof(IdentityConstants.ApplicationScheme))]
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
        private readonly ApiDbContext context;

        public GalleryController(ApiDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gallery>>> GetGalleries()
        {
            return await context.Galleries.Where(g => !g.IsPrivate).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Gallery>> GetGallery(int id)
        {
            Gallery gallery = await context.Galleries.FindAsync(id);

            if (gallery == null)
                return NotFound();

            return gallery;
        }

        [HttpPost]
        public async Task<ActionResult<Gallery>> CreateGallery(Gallery gallery)
        {
            context.Galleries.Add(gallery);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGallery), new { id = gallery.Id }, gallery);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGallery(int id, Gallery gallery)
        {
            if (id != gallery.Id)
                return BadRequest();

            context.Galleries.Update(gallery);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GalleryExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGallery(int id)
        {
            Gallery gallery = await context.Galleries.FindAsync(id);
            if (gallery == null)
                return NotFound();

            context.Galleries.Remove(gallery);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool GalleryExists(int id)
        {
            return context.Galleries.Any(e => e.Id == id);
        }
    }