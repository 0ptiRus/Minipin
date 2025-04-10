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
        private readonly FileService service;
        private readonly MinioService minio_service;
        private readonly ILogger<FilesController> logger;

        public FilesController(FileService service, MinioService minio_service, ILogger<FilesController> logger)
        {
            this.service = service;
            this.minio_service = minio_service;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles()
        {
            logger.LogInformation($"Retrieving all files");
            var files = await service.GetFiles();
            logger.LogInformation($"Found {files.Count} files");
            return Ok(files);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFile(int id)
        {
            logger.LogDebug($"Attempting to retrieve file with ID: {id}");
            var file = await service.GetFile(id);

            if (file != null)
            {
                logger.LogInformation($"File {id} found successfully");
                return Ok(file);
            }

            logger.LogWarning($"File {id} not found");
            return NotFound();
        }

        [HttpGet("{content_type}/{object_name}")]
        public async Task<IActionResult> GetFileUrl(string content_type, string objectName)
        {
            logger.LogInformation($"Generating URL for object: {objectName}");
            var url = await minio_service.GetFileUrlAsync(objectName, content_type);

            if (url != null)
            {
                logger.LogInformation($"URL generated successfully for object: {objectName}");
                return Ok(url);
            }

            logger.LogWarning($"Object {objectName} not found in MinIO");
            return NotFound();
        }

        [HttpPost("upload")] // Fix conflicting [HttpPost] routes
        public async Task<IActionResult> PostFile([FromBody] IFormFile file, [FromQuery] int? post_id=null,
            [FromQuery] int? gallery_id=null, [FromQuery] string? user_id=null)
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
                UploadedFile new_file = new UploadedFile
                {
                    ObjectName = object_name,
                    GalleryId = gallery_id,
                    PostId = post_id,
                    UserId = user_id
                };
                
                await service.CreateFile(new_file, file, file.ContentType);

                logger.LogInformation($"File {new_file.Id} uploaded successfully");
                return CreatedAtAction(nameof(GetFile), new { id = new_file.Id }, new_file);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error uploading file");
                return StatusCode(500);
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> PutFile(UploadedFile uploadedFile)
        {
            try
            {
                logger.LogInformation($"Updating image {uploadedFile.Id}");

                uploadedFile = await service.UpdateFile(uploadedFile);

                logger.LogInformation($"Image {uploadedFile.Id} updated successfully");
                return Ok(uploadedFile);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError(ex, $"Concurrency error updating image {uploadedFile.Id}");
                if (await service.GetFile(uploadedFile.Id) is null)
                {
                    logger.LogWarning($"Image {uploadedFile.Id} does not exist");
                    return NotFound();
                }
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            logger.LogInformation($"Attempting to delete File {id}");
            var image = await service.GetFile(id);

            if (image == null)
            {
                logger.LogWarning($"File {id} not found");
                return NotFound();
            }

            try
            {
                logger.LogInformation($"Deleting object {image.ObjectName} from MinIO");
                await minio_service.DeleteFileAsync(image.ObjectName, image.ContentType);


                if (await service.DeleteFile(image))
                {
                    logger.LogInformation($"File {id} deleted successfully");
                    return Ok();   
                }
                logger.LogError($"Error deleting file {id}");
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting image {id}");
                return StatusCode(500);
            }
        }
    }
}