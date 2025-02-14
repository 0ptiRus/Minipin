namespace exam_api.Data;

public class Gallery
{
    public int Id { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public ICollection<Image> Images { get; set; }
}