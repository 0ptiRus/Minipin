using System;
using System.Collections.Generic;

namespace exam_api.Models;

public class CommentModel
{
    public int Id { get; set; }
    
    public int PostId { get; set; }
    
    public string Username { get; set; }
    public string UserId { get; set; }
    public int? ParentCommentId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ProfilePictureUrl { get; set; }
    public IList<CommentModel>? Replies { get; set; }
}