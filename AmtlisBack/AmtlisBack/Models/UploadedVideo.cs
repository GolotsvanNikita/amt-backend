using System.ComponentModel.DataAnnotations;

namespace AmtlisBack.Models
{
    public class UploadedVideo
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        [Required]
        public string VideoUrl { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = "/1v.png";

        public int Views { get; set; } = 0;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsReel { get; set; } = false;

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}