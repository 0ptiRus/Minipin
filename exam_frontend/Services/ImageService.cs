using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using exam_frontend.Entities;

namespace exam_frontend.Controllers;

    public class ImageService
    {
        private readonly AppDbContext context;
        private readonly IWebHostEnvironment env;
        private const string FileFolder = "uploads";

        public ImageService(AppDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
        }

        public async Task<IEnumerable<Image>> GetImages()
        {
            return await context.Images.ToListAsync();
        }

        public async Task<Image> GetImage(int id)
        {
            Image image = await context.Images
                .Include(i => i.Comments)
                .Include(i => i.Likes)
                .FirstOrDefaultAsync(i => i.Id == id);

            return image;
        }
        public async Task<Image> PostImage(IFormFile file, [FromForm] int gallery_id)
        {
            if (file == null || file.Length == 0)
                return null;

            string folder = Path.Combine("wwwroot", FileFolder);
            
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string file_name = $"{Guid.NewGuid()}_{file.FileName}";
            string file_path = Path.Combine(folder, file_name);

            using (FileStream stream = new(file_path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Image image = new(Path.Combine(FileFolder, file_name), gallery_id);

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

        public async Task<bool> DeleteImage(int id)
        {
            Image image = await context.Images.FindAsync(id);
            if (image == null)
            return false;

            context.Images.Remove(image);
            await context.SaveChangesAsync();
            
            System.IO.File.Delete(image.FilePath);

        return true;
        }

        private bool ImageExists(int id)
        {
            return context.Images.Any(e => e.Id == id);
        }
    }