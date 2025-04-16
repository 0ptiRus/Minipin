using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace exam_frontend.Models;

public class CreatePostModel
{
    public IFormFile File { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int GalleryId { get; set; }
    [ValidateNever]
    public string UserId { get; set; }
}