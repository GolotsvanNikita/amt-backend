namespace AmtlisBack.Models
{
    public class PlaylistDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int VideoCount { get; set; } = 0;
    }
}