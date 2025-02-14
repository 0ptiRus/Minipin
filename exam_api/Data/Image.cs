namespace exam_api.Data;

public class Image
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public byte[] Content { get; set; }
    public string ContentType { get; set; }
    
    public int GalleryId { get; set; }
    public Gallery Gallery { get; set; }
    
    public ICollection<Comment> Comments { get; set; }
    public ICollection<Like> Likes { get; set; }
}