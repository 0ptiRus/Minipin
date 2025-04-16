namespace exam_api.Entities;

public class Post
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public Gallery Gallery { get; set; }
    public int GalleryId { get; set; }
    
    public ApplicationUser User { get; set; }
    public string UserId { get; set; }
    
    public UploadedFile Upload { get; set; }
    
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Like> Likes { get; set; }

    public Post(string name, string description, int galleryId, string userId)
    {
        Name = name;
        Description = description;
        GalleryId = galleryId;
        UserId = userId;
    }
}