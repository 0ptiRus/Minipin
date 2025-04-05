namespace exam_frontend.Models;

public class CommentModel
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int ImageId { get; set; }
    public string UserId { get; set; }

    public CommentModel(int id, string text, int imageId, string userId)
    {
        Id = id;
        Text = text;
        ImageId = imageId;
        UserId = userId;
    }
}