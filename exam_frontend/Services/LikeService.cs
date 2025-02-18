using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_frontend.Controllers;

public class LikeService
{
    private readonly AppDbContext context;

    public LikeService(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Like>> GetLikes()
    {
        return await context.Likes.ToListAsync();
    }

    public async Task<Like> GetLike(int id)
    {
        Like like = await context.Likes.FindAsync(id);
        return like;
    }
    
    public async Task<bool> LikeImage(int id, string user_id)
    {
        Image image = await context.Images.FindAsync(id);
        if (image == null)
            return false;

        Like like = new Like
        {
            ImageId = id,
            UserId = user_id
        };

        image.Likes.Add(like);
        await context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> UnlikeImage(int id, string user_id)
    {
        Like like = await context.Likes.FirstOrDefaultAsync(l => l.ImageId == id && l.UserId == user_id);
        if (like == null)
            return false;

        context.Likes.Remove(like);
        await context.SaveChangesAsync();

        return true;
    }
}