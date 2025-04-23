namespace exam_api.Models;

public class ViewUserModel
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Pfp { get; set; }
    public string Role { get; set; }
    public bool IsBanned { get; set; }
}