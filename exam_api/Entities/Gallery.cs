using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace exam_api.Entities;

public class Gallery
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public UploadedFile Cover { get; set; }
    public IList<Post> Posts { get; set; }
    public IList<SavedPost> SavedPosts { get; set; }

    public Gallery(string name, string userId, bool isPrivate)
    {
        Name = name;
        UserId = userId;
        IsPrivate = isPrivate;
    }
    
    public Gallery(string name, string description, string userId, bool isPrivate)
    {
        Name = name;
        Description = description;
        UserId = userId;
        IsPrivate = isPrivate;
    }
}