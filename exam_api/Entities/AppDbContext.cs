using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Entities;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<UploadedFile> Files { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }

    public DbSet<Follow> Follows { get; set; }
    public DbSet<Post> Posts { get; set; }
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowerId);

        builder.Entity<Follow>()
            .HasOne(f => f.Followed)
            .WithMany(u => u.Followed)
            .HasForeignKey(f => f.FollowedId);
    }
}