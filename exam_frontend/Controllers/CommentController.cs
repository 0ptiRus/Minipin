using exam_frontend.Entities;
using exam_frontend.Models;
using exam_frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace exam_frontend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CommentController : ControllerBase
{
    private readonly CommentService service;

    public CommentController(CommentService service)
    {
        this.service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Comment(int image_id, string user_id, string text)
    {
        Comment comment = await service.PostComment(image_id, user_id, text);
        if(comment is not null)
            return new JsonResult(new CommentModel
            {
                Id=comment.Id, ImageId = comment.ImageId,
                Text = comment.Text, UserId = comment.UserId
            });
        return BadRequest();
    }
    
}