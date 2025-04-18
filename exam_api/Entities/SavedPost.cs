namespace exam_api.Entities;

public class SavedPost
{
    public int Id { get; set; }
    
    public int PostId { get; set; }
    public Post Post { get; set; }

    public int GalleryId { get; set; }
    public Gallery Gallery { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
