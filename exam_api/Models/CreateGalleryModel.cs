using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace exam_api.Models;

public class CreateGalleryModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsPrivate { get; set; }
    public IFormFile Image { get; set; }
    public string UserId { get; set; }
}