using System.Collections;
using exam_api.Entities;

namespace exam_api.Models;

public class GalleryDetailsModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Username { get; set; }
    public string UserId { get; set; }
    public string CoverUrl { get; set; }
    
    public string Pfp { get; set; }
    public IList<PreviewPostModel> Posts { get; set; }
    public int PostsCount { get; set; }
    public int CommentsCount { get; set; }
}