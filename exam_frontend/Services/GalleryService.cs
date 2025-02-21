using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_frontend.Services;

public class GalleryService
{
    private readonly AppDbContext context;

    public GalleryService(AppDbContext context)
    {
        this.context = context;
    }
    public async Task<IEnumerable<Gallery>> GetGalleries()
    {
        return await context.Galleries.Where(g => !g.IsPrivate).ToListAsync();
    }

    public async Task<Gallery> GetGallery(int id)
    {
        Gallery gallery = await context.Galleries.FindAsync(id);

        return gallery;
    }

    public async Task<Gallery> GetUserGallery(string user_id, int id)
    {
        return await context.Galleries
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == user_id);
    }
    public async Task<Gallery> GetGalleryWithImages(string user_id, int id)
    {
        Gallery gallery = await context.Galleries
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == user_id);
        await context.Entry(gallery)
            .Collection(p => p.Images)
            .LoadAsync();
        return gallery;
    }
    public async Task<IList<Gallery>> GetUserGalleries(string user_id)
    {
        return await context.Galleries
            .Where(g => g.UserId == user_id)
            .Include(g => g.Images)
            .ToListAsync();
    }

    public async Task<IList<Gallery>> GetFeed(string user_id)
    {
        return await context.Galleries
            .Where(g => context.Follows
                .Where(f => f.FollowerId == user_id)
                .Select(f => f.FollowedId)
                .Contains(g.UserId)
            )
            .Where(g => !g.IsPrivate)
            .Include(g => g.Images)
            .Where(g => g.Images.Count > 0)
            .ToListAsync();
    }

    public async Task<Gallery> CreateGallery(Gallery gallery)
    {
        context.Galleries.Add(gallery);
        await context.SaveChangesAsync();

        return gallery;
    }

    public async Task<Gallery> UpdateGallery(int id, Gallery gallery)
    {
        if (id != gallery.Id)
            return null;

        context.Galleries.Update(gallery);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GalleryExists(id))
                return null; 
            else
                throw;
        }

        return gallery;
    }

    public async Task<bool> DeleteGallery(int id)
    {
        Gallery gallery = await context.Galleries.FindAsync(id);
        if (gallery == null)
            return false;
        context.Galleries.Remove(gallery);
        await context.SaveChangesAsync();
        return true;
    }

    private bool GalleryExists(int id)
    {
        return context.Galleries.Any(e => e.Id == id);
    }
}