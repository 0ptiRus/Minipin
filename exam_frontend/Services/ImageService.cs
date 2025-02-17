using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using exam_frontend.Entities;

namespace exam_frontend.Controllers;

    public class ImageService
    {
        private readonly ApiDbContext context;

        public ImageService(ApiDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Image>> GetImages()
        {
            return await context.Images.ToListAsync();
        }

        public async Task<Image> GetImage(int id)
        {
            Image image = await context.Images.FindAsync(id);

            return image;
        }
        public async Task<Image> PostImage(IFormFile file, [FromForm] int gallery_id)
        {
        if (file == null || file.Length == 0)
            return null;

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string file_name = $"{Guid.NewGuid()}_{file.FileName}";
            string path = Path.Combine(folder, file_name);

            using (FileStream stream = new(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Image image = new(path, gallery_id);

            context.Images.Add(image);
            await context.SaveChangesAsync();

        return image;
        }

        public async Task<Image> PutImage(int id, Image image)
        {
        if (id != image.Id)
            return null;

            context.Images.Update(image);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
            if (!ImageExists(id))
                return null;
            else
                throw;
            }

        return image;
        }

        public async Task DeleteImage(int id)
        {
            Image image = await context.Images.FindAsync(id);
            if (image == null)
        {

        }

            context.Images.Remove(image);
            await context.SaveChangesAsync();
            
            System.IO.File.Delete(image.FilePath);
        }

        private bool ImageExists(int id)
        {
            return context.Images.Any(e => e.Id == id);
        }
    }