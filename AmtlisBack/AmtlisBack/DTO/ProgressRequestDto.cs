public class ProgressRequestDto
{
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelAvatarUrl { get; set; } = "/ava.png";
    public string ViewsCount { get; set; } = "0";
    public string LikesCount { get; set; } = "0";
    public int DurationSeconds { get; set; }
    public int LastPositionSeconds { get; set; }
}