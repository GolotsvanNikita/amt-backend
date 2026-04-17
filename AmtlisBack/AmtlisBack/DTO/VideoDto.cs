namespace AmtlisBack.Models
{
    public class VideoDto
    {
        public string Id { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Views { get; set; } = string.Empty;
        public string PublishedAt { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Likes { get; set; } = "0";
        public string CommentsCount { get; set; } = "0";
        public string ChannelAvatarUrl { get; set; } = "/ava.png";
    }
}