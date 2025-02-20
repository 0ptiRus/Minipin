namespace exam_frontend.Models;

public class UserModel
{
    public string Id { get; set; }
    public string Username { get; set; }

    public UserModel(string id, string username)
    {
        Id = id;
        Username = username;
    }
}