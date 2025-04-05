namespace exam_api.Models;

public class CommentModel
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int ImageId { get; set; }
    public string UserId { get; set; }
}