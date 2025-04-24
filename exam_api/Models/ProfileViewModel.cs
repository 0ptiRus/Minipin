using exam_api.Entities;

namespace exam_api.Models;

public class ProfileViewModel
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string PfpUrl { get; set; }
    public int PinCount { get; set; }
    public int GalleryCount { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    
    // New property for posts
    public ICollection<PreviewGalleryModel> Galleries { get; set; }
}