namespace exam_api.Models;

public class AdminCommentModel
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string Username { get; set; }
    public string CommentPostImage { get; set; }
    public string CommentPostName { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsDeleted { get; set; }
}