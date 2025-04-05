namespace exam_frontend.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    
    public int PostId { get; set; }
    public Post Post { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}