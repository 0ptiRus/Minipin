using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace exam_frontend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class LikeController : ControllerBase
{
    private readonly LikeService service;
    private readonly ImageService image_service;

    public LikeController(LikeService service, ImageService imageService)
    {
        this.service = service;
        image_service = imageService;
    }

    [HttpPost]
    public async Task<IActionResult> Like(int image_id, string user_id)
    {
        LikeService.LikeResponse like_status = await service.LikeImage(image_id, user_id);
        if (!like_status.IsLiked && !like_status.IsUnliked)
            return BadRequest("Couldn't like or dislike image!");
        return new JsonResult(like_status);
    }
}
