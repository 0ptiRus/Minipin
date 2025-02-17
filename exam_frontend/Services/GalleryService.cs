using exam_frontend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exam_frontend.Services;

public class GalleryService
{
    private readonly ApiDbContext context;

    public GalleryService(ApiDbContext context)
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

    public async Task DeleteGallery(int id)
    {
        Gallery gallery = await context.Galleries.FindAsync(id);
        if (gallery == null)

        context.Galleries.Remove(gallery);
        await context.SaveChangesAsync();
    }

    private bool GalleryExists(int id)
    {
        return context.Galleries.Any(e => e.Id == id);
    }
}