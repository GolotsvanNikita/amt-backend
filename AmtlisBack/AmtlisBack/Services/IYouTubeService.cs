using AmtlisBack.Models;

namespace AmtlisBack.Services
{
    public class YouTubeResponse
    {
        public List<VideoDto> Videos { get; set; } = new();
        public string NextPageToken { get; set; } = string.Empty;

    }

    public interface IYouTubeService
    {
        Task<YouTubeResponse> GetVideosAsync(int maxResults, string pageToken = "", string categoryId = "");
        Task<YouTubeResponse> GetShortsAsync(int maxResults);
        Task<List<CommentDto>> GetVideoCommentsAsync(string videoId, int maxResults = 20);
        Task<ChannelDto?> GetChannelAsync(string channelId);
        Task<YouTubeResponse> GetVideosFromPlaylistAsync(string playlistId, int maxResults = 10);
        Task<List<PlaylistDto>> GetChannelPlaylistsAsync(string channelId, int maxResults = 5);
        Task<YouTubeResponse> SearchVideosAsync(string query, int maxResults = 15, string pageToken = "");
    }
}