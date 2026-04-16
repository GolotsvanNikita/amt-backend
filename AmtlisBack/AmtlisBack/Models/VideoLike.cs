using System.ComponentModel.DataAnnotations;

namespace AmtlisBack.Models
{
    public class VideoLike
    {
        public int Id { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public string VideoId { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
    }
}