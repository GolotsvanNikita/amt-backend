using System.ComponentModel.DataAnnotations;

namespace AmtlisBack.Models
{
    public class WatchHistory
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string VideoId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "/ava.png";
        public string ChannelName { get; set; } = string.Empty;

        public DateTime WatchedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}