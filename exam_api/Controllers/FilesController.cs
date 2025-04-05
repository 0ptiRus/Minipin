using exam_api.Entities;
using exam_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace exam_api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly MinioService minio_service;
        private readonly ILogger<FilesController> logger;

        public FilesController(AppDbContext context, MinioService service, ILogger<FilesController> logger)
        {
            this.context = context;
            minio_service = service;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles()
        {
            logger.LogInformation($"Retrieving all images");
            var images = await context.Files.ToListAsync();
            logger.LogInformation($"Found {images.Count} images");
            return Ok(images);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFile(int id)
        {
            logger.LogDebug($"Attempting to retrieve file with ID: {id}");
            var image = await context.Files
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image != null)
            {
                logger.LogInformation($"File {id} found successfully");
                return Ok(image);
            }

            logger.LogWarning($"File {id} not found");
            return NotFound();
        }

        [HttpGet("{object_name}")]
        public async Task<IActionResult> GetFileUrl(string objectName)
        {
            logger.LogInformation($"Generating URL for object: {objectName}");
            var url = await minio_service.GetFileUrlAsync(objectName);

            if (url != null)
            {
                logger.LogInformation($"URL generated successfully for object: {objectName}");
                return Ok(url);
            }

            logger.LogWarning($"Object {objectName} not found in MinIO");
            return NotFound();
        }

        [HttpPost("upload")] // Fix conflicting [HttpPost] routes
        public async Task<IActionResult> PostFile([FromBody] IFormFile file, [FromQuery] int? post_id,
            [FromQuery] int? gallery_id, [FromQuery] string? user_id)
        {
            if (file == null || file.Length == 0)
            {
                logger.LogWarning($"Invalid file upload attempt (empty file)");
                return BadRequest("File is required");
            }

            try
            {
                logger.LogInformation($"Uploading new file");
                string object_name = $"{Guid.NewGuid()}_{file.FileName}";
                await minio_service.UploadFileAsync(object_name, file.OpenReadStream(), file.ContentType);

                UploadedFile new_file = new UploadedFile
                {
                    ObjectName = object_name,
                    GalleryId = gallery_id,
                    PostId = post_id,
                    UserId = user_id
                };
                context.Files.Add(new_file);
                await context.SaveChangesAsync();

                logger.LogInformation($"File {new_file.Id} uploaded successfully");
                return CreatedAtAction(nameof(GetFile), new { id = new_file.Id }, new_file);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error uploading image for gallery {post_id}");
                return StatusCode(500);
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> PutFile(UploadedFile uploadedFile)
        {
            try
            {
                logger.LogInformation($"Updating image {uploadedFile.Id}");
                context.Entry(uploadedFile).State = EntityState.Modified;
                await context.SaveChangesAsync();

                logger.LogInformation($"Image {uploadedFile.Id} updated successfully");
                return Ok(uploadedFile);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError(ex, $"Concurrency error updating image {uploadedFile.Id}");
                if (!FileExists(uploadedFile.Id))
                {
                    logger.LogWarning($"Image {uploadedFile.Id} does not exist");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFILE(int id)
        {
            logger.LogInformation($"Attempting to delete image {id}");
            var image = await context.Files.FindAsync(id);

            if (image == null)
            {
                logger.LogWarning($"Image {id} not found");
                return NotFound();
            }

            try
            {
                logger.LogInformation($"Deleting object {image.ObjectName} from MinIO");
                await minio_service.DeleteFileAsync(image.ObjectName);

                context.Files.Remove(image);
                await context.SaveChangesAsync();

                logger.LogInformation($"Image {id} deleted successfully");
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting image {id}");
                return StatusCode(500);
            }
        }

        private bool FileExists(int id)
        {
            logger.LogDebug($"Checking existence of image with ID: {id}");
            return context.Files.Any(e => e.Id == id);
        }
    }
}