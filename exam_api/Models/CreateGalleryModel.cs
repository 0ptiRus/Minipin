using System.ComponentModel.DataAnnotations;

namespace exam_api.Models;

public class CreateGalleryModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    public bool IsPrivate { get; set; }
    public string UserId { get; set; }
}