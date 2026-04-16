using System.Data;

namespace AmtlisBack.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Username { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "/ava.png";
        public string BannerUrl { get; set; } = "/backimage.jpg";
        public int SubscribersCount { get; set; } = 0;

        public ICollection<WatchHistory> WatchHistories { get; set; } = [];
    }
}
