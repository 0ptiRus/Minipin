namespace exam_frontend.Entities;

public class Gallery
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public ICollection<Image> Images { get; set; }

    public Gallery(string name, string userId, bool isPrivate)
    {
        Name = name;
        UserId = userId;
        IsPrivate = isPrivate;
    }
}