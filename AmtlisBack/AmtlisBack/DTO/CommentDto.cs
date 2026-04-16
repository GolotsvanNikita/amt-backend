namespace AmtlisBack.Models
{
    public class CommentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string? ParentId { get; set; }
        public List<CommentDto> Replies { get; set; } = [];
    }
}