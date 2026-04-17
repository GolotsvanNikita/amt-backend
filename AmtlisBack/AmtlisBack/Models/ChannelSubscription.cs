using System.ComponentModel.DataAnnotations;

namespace AmtlisBack.Models
{
    public class ChannelSubscription
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string ChannelId { get; set; } = string.Empty;
        [Required]
        public string ChannelName { get; set; }
        [Required]
        public string AvatarUrl { get; set; } = "/ava.png";
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
    }
}