using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace exam_api.Entities;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<UploadedFile> Files { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }

    public DbSet<Follow> Follows { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<SavedPost> SavedPosts { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Followed) // User is following others
            .HasForeignKey(f => f.FollowerId);

        builder.Entity<Follow>()
            .HasOne(f => f.Followed)
            .WithMany(u => u.Followers) // User is being followed by others
            .HasForeignKey(f => f.FollowedId);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Followers);
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Followed);
        
        builder.Entity<SavedPost>()
            .HasOne(sp => sp.Post)
            .WithMany(p => p.SavedInGalleries)
            .HasForeignKey(sp => sp.PostId);

        builder.Entity<SavedPost>()
            .HasOne(sp => sp.Gallery)
            .WithMany(g => g.SavedPosts)
            .HasForeignKey(sp => sp.GalleryId);
        
    }
}