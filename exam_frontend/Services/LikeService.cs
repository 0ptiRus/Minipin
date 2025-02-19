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
    
    public async Task<LikeResponse> LikeImage(int id, string user_id)
    {
        if (await context.Likes
                .SingleOrDefaultAsync(l => l.ImageId == id
                                           && l.UserId == user_id) != null)
        {
            if (await UnlikeImage(id, user_id))
            {
                return new LikeResponse { IsLiked = false, IsUnliked = true};
            }
        }
        
        Image image = await context.Images
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (image == null)
            return new LikeResponse { IsLiked = false, IsUnliked = false};

        Like like = new Like
        {
            ImageId = id,
            UserId = user_id
        };

        image.Likes.Add(like);
        await context.SaveChangesAsync();

        Console.WriteLine("True and false");
        return new LikeResponse { IsLiked = true, IsUnliked = false};
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

    public class LikeResponse
    {
        public bool IsLiked { get; set; }
        public bool IsUnliked { get; set; }
        
    }
    
}