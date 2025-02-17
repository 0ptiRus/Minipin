namespace exam_frontend.Entities;

public class Image
{
    public int Id { get; set; }
    public string FilePath { get; set; }
    
    public int GalleryId { get; set; }
    public Gallery Gallery { get; set; }
    
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Like> Likes { get; set; }

    public Image(string filePath, int galleryId)
    {
        FilePath = filePath;
        GalleryId = galleryId;
    }
}