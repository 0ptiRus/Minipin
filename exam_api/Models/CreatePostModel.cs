using Microsoft.AspNetCore.Http;

namespace exam_api.Models;

public class CreatePostModel
{
    public IFormFile File { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string UserId { get; set; }
    public int GalleryId { get; set; }
    public string Tags { get; set; }
}