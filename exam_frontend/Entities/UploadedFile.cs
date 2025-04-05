namespace exam_frontend.Entities;

public class UploadedFile
{
    public int Id { get; set; }
    public string ObjectName { get; set; }
    
    public int? PostId { get; set; }
    public Post Post { get; set; }
    
    public int? GalleryId { get; set; }
    public Gallery Gallery { get; set; }
    
    public string? UserId { get; set; }
    public ApplicationUser User { get; set; }

    public UploadedFile(string objectName, int postId)
    {
        ObjectName = objectName;
        PostId = postId;
    }
}