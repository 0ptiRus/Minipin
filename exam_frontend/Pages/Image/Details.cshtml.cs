using exam_frontend.Controllers;
using exam_frontend.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Image;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class Details : PageModel
{
    private ImageService service;
    public ImageItem Image { get; set; }
    public List<Comment> Comments { get; set; }

    public Details(ImageService service)
    {
        this.service = service;
    }

    public async void OnGet(int image_id)
    {
        Entities.Image image = await service.GetImage(image_id);

        Image = new(image.Id, image.FilePath, image.Likes.Count, image.GalleryId);

        Comments = image.Comments.ToList();
    }
}

public class ImageItem(int Id, string Url, int Likes, int GalleryId)
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int Likes { get; set; }
    public int GalleryId { get; set; }
}