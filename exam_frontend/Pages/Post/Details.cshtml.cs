using System.Security.Claims;
using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace exam_frontend.Pages.Post;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using exam_frontend.Entities;

[Authorize]
public class Details : PageModel
{
    public readonly MinioService minio;
    private readonly IApiService api;
    [BindProperty] public Post Post { get; set; }
    public IList<CommentModel> Comments { get; set; } = new List<CommentModel>();

    public Details(IApiService api, MinioService minio)
    {
        this.minio = minio;
        this.api = api;
    }

    public async void OnGet(int post_id)
    {
        //Image = await image_service.GetImage(image_id);
        HttpResponseMessage response = await api.GetAsync($"Posts/{post_id}");
        if(response.IsSuccessStatusCode)
            Post = api.JsonToContent<Post>(await response.Content.ReadAsStringAsync());
        foreach (Entities.Comment comment in Post.Comments)
        {
            Comments.Add(new CommentModel
            (
                comment.Id, comment.Text,
                comment.PostId, comment.UserId
            ));
        }
    }
}

