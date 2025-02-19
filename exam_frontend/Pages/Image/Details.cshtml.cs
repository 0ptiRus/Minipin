using System.Security.Claims;
using exam_frontend.Controllers;
using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Image;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class Details : PageModel
{
    private readonly ImageService image_service;
    private readonly LikeService like_service;
    private readonly CommentService comment_service;
    
    [BindProperty(SupportsGet = true)]
    public int Image_Id { get; set; }
    [BindProperty] public Entities.Image Image { get; set; }
    public IList<CommentModel> Comments { get; set; } = new List<CommentModel>();

    public Details(ImageService imageService, LikeService likeService, CommentService commentService)
    {
        image_service = imageService;
        like_service = likeService;
        comment_service = commentService;
    }

    public async void OnGet(int image_id)
    {
        Image = await image_service.GetImage(image_id);
        foreach (Entities.Comment comment in Image.Comments)
        {
            Comments.Add(new CommentModel
            {
                Id = comment.Id, Text = comment.Text,
                ImageId = comment.ImageId, UserId = comment.UserId
            });
        }
    }
}

