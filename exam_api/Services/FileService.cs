using exam_api.Entities;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Services;

public class FileService
{
    private readonly AppDbContext context;
    private readonly MinioService minio;

    public FileService(AppDbContext context, MinioService minio)
    {
        this.context = context;
        this.minio = minio;
    }

    public async Task<UploadedFile> CreateFile(UploadedFile file, IFormFile uploaded_file)
    {
        await context.Files.AddAsync(file);
        await context.SaveChangesAsync(); 
        
        await minio.UploadFileAsync(file.ObjectName, uploaded_file.OpenReadStream(), uploaded_file.ContentType);
        return file;
    }

    public async Task<IList<UploadedFile>> GetFiles() => await context.Files.ToListAsync();
    public async Task<UploadedFile> GetFile(int id) => await context.Files.FindAsync(id);

    public async Task<UploadedFile> UpdateFile(UploadedFile file)
    {
        context.Entry(file).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return file;
    }
    
    public async Task<bool> DeleteFile(UploadedFile file)
    {
        context.Files.Remove(file);
        await context.SaveChangesAsync();
        return true;
    }
}