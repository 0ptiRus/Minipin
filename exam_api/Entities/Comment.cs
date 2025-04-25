using System;
using System.Collections.Generic;

namespace exam_api.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int? ParentCommentId { get; set; } 
    public Comment ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}