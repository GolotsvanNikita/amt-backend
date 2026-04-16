using System.ComponentModel.DataAnnotations;

namespace AmtlisBack.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public string VideoId { get; set; }
        [Required] public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ParentId { get; set; }

        public User? User { get; set; }
    }
}