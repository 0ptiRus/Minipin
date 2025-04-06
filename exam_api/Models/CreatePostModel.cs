namespace exam_api.Models;

public class CreatePostModel
{
    public IFormFile File { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int GalleryId { get; set; }
}