using exam_api.Entities;

namespace exam_api.Models;

public class ProfileViewModel
{
    public ApplicationUser User { get; set; }
    public int PinCount => User.Posts.Count(); // Assuming "Posts" is the collection name
    public int GalleryCount => User.Galleries.Count();
    public int FollowerCount => User.Followers.Count();
    public int FollowingCount => User.Followed.Count();
    
    // New property for posts
    public ICollection<Gallery> Galleries { get; set; }
}