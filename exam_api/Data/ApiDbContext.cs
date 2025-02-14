using Microsoft.EntityFrameworkCore;

namespace exam_api.Data;

public class ApiDbContext : DbContext
{
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }

    public ApiDbContext()
    {
    }

    public ApiDbContext(DbContextOptions options) : base(options)
    {
    }
}