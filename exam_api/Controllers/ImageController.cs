using exam_api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Controllers;

[Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ApiDbContext context;

        public ImageController(ApiDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages()
        {
            return await context.Images.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            Image image = await context.Images.FindAsync(id);

            if (image == null)
                return NotFound();

            return image;
        }

        [HttpPost]
        public async Task<ActionResult<Image>> PostImage([FromForm]IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            Image image = new Image
            {
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                image.Content = memoryStream.ToArray();
            }

            context.Images.Add(image);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutImage(int id, Image image)
        {
            if (id != image.Id)
                return BadRequest();

            context.Images.Update(image);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            Image image = await context.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            context.Images.Remove(image);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ImageExists(int id)
        {
            return context.Images.Any(e => e.Id == id);
        }
    }