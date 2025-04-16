using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace exam_frontend.Models;

public class CommentModel
{
    public int Id { get; set; }
    
    public int PostId { get; set; }
    
    public string Username { get; set; }
    public string UserId { get; set; }
    
    public bool IsReplying { get; set; }
    public int? ParentCommentId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    [ValidateNever]
    public string ProfilePictureUrl { get; set; }
    public string ReplyText { get; set; }
    public IList<CommentModel>? Replies { get; set; }
}