using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace exam_frontend.Models;

public class CommentModel
{
    public int Id { get; set; }
    
    public int PostId { get; set; }
    
    public string UserId { get; set; }
    public string Text { get; set; }
    public string UserName { get; set; }
    [ValidateNever]
    public string ProfilePictureUrl { get; set; }
}