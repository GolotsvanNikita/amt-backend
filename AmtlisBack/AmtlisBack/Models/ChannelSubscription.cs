using System.ComponentModel.DataAnnotations;

namespace AmtlisBack.Models
{
    public class ChannelSubscription
    {
        public int Id { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public string ChannelName { get; set; }
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
    }
}