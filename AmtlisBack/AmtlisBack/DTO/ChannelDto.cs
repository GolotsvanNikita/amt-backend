namespace AmtlisBack.Models
{
    public class ChannelDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
        public string SubscriberCount { get; set; } = "0";
        public string CustomUrl { get; set; } = string.Empty;
        public string UploadsPlaylistId { get; set; } = string.Empty;
    }
}