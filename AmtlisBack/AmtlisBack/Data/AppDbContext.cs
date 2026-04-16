using AmtlisBack.Models;
using Microsoft.EntityFrameworkCore;

namespace AmtlisBack.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<WatchHistory> WatchHistories { get; set; }
        public DbSet<UploadedVideo> UploadedVideos { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<VideoLike> VideoLikes { get; set; }
        public DbSet<ChannelSubscription> Subscriptions { get; set; }
    }
}
