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

        public int DurationSeconds { get; set; }
        public int LastPositionSeconds { get; set; }
        public int ProgressPercent { get; set; }
        public bool IsFinished { get; set; }

        public string ChannelId { get; set; } = string.Empty;
        public string ViewsCount { get; set; } = "0";
        public string LikesCount { get; set; } = "0";

        public User? User { get; set; }
    }
}