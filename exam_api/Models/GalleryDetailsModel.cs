using System.Collections;
using exam_api.Entities;

namespace exam_api.Models;

public class GalleryDetailsModel
{
    public Gallery Gallery { get; set; }
    public string CoverUrl { get; set; }
    public string Pfp { get; set; }
    public IList<string> ImageUrls { get; set; }
    public int PostsCount => Gallery.Posts.Count;
    public int CommentsCount => Gallery.Posts.Sum(p => p.Comments.Count);
}