using System.Text.Json;
using AmtlisBack.Models;

namespace AmtlisBack.Services
{
    public class YouTubeService : IYouTubeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public YouTubeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["YouTubeApi:ApiKey"];
        }

        public async Task<YouTubeResponse> GetVideosAsync(int maxResults, string pageToken = "", string categoryId = "")
        {
            var url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&chart=mostPopular&maxResults={maxResults}&regionCode=US&key={_apiKey}";

            if (!string.IsNullOrEmpty(pageToken))
            {
                url += $"&pageToken={pageToken}";
            }

            if (!string.IsNullOrEmpty(categoryId))
            {
                url += $"&videoCategoryId={categoryId}";
            }

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new YouTubeResponse();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonString);
            var items = document.RootElement.GetProperty("items");

            string nextToken = document.RootElement.TryGetProperty("nextPageToken", out var tokenProp) ? tokenProp.GetString() ?? "" : "";

            var videos = new List<VideoDto>();
            foreach (var item in items.EnumerateArray())
            {
                var snippet = item.GetProperty("snippet");
                var statistics = item.GetProperty("statistics");
                string viewsCount = statistics.TryGetProperty("viewCount", out var vCount) ? vCount.GetString() : "0";

                videos.Add(new VideoDto
                {
                    Id = item.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                    Title = snippet.GetProperty("title").GetString() ?? "Unknown Title",
                    ChannelName = snippet.GetProperty("channelTitle").GetString() ?? "Unknown Channel",
                    ThumbnailUrl = snippet.GetProperty("thumbnails").GetProperty("medium").GetProperty("url").GetString() ?? "",
                    Views = viewsCount,
                    PublishedAt = snippet.GetProperty("publishedAt").GetDateTime().ToString("MMM dd, yyyy")
                });
            }

            return new YouTubeResponse { Videos = videos, NextPageToken = nextToken };
        }
    }
}