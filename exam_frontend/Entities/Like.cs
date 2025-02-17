namespace exam_frontend.Entities;

public class Like
{
    public int Id { get; set; }
    
    public int ImageId { get; set; }
    public Image Image { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}