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
                var statistics = item.TryGetProperty("statistics", out var stats) ? stats : default;

                string viewsCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("viewCount", out var vCount)
                    ? FormatViews(vCount.GetString())
                    : "0";

                string likesCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("likeCount", out var lCount)
                    ? FormatViews(lCount.GetString()!)
                    : "0";

                if (viewsCount == "0")
                {
                    continue;
                }

                var thumbnails = snippet.GetProperty("thumbnails");

                bool hasHigh = thumbnails.TryGetProperty("high", out var high);
                bool hasMedium = thumbnails.TryGetProperty("medium", out var med);

                if (!hasHigh && !hasMedium)
                {
                    continue;
                }

                string? thumbUrl = hasHigh
                    ? high.GetProperty("url").GetString()
                    : med.GetProperty("url").GetString();

                videos.Add(new VideoDto
                {
                    Id = item.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                    Title = snippet.GetProperty("title").GetString() ?? "Unknown Title",
                    ChannelName = snippet.GetProperty("channelTitle").GetString() ?? "Unknown Channel",
                    ThumbnailUrl = thumbUrl ?? "",
                    Views = viewsCount + " views",
                    Likes = likesCount,
                    PublishedAt = snippet.GetProperty("publishedAt").GetDateTime().ToString("MMM dd, yyyy"),
                    Description = snippet.TryGetProperty("description", out JsonElement desc) ? desc.GetString() : ""
                });
            }

            return new YouTubeResponse { Videos = videos, NextPageToken = nextToken };
        }
        private string FormatViews(string views)
        {
            if (long.TryParse(views, out long count))
            {
                if (count >= 1000000) return (count / 1000000D).ToString("0.#") + "M";
                if (count >= 1000) return (count / 1000D).ToString("0.#") + "K";
            }
            return views;
        }

        public async Task<YouTubeResponse> GetShortsAsync(int maxResults)
        {
            var searchUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q=%23shorts&type=video&videoDuration=short&maxResults={maxResults}&key={_apiKey}";
            var searchResponse = await _httpClient.GetAsync(searchUrl);

            if (!searchResponse.IsSuccessStatusCode) return new YouTubeResponse();

            var searchJson = await searchResponse.Content.ReadAsStringAsync();
            using var searchDoc = JsonDocument.Parse(searchJson);
            var searchItems = searchDoc.RootElement.GetProperty("items");

            var videoIds = new List<string>();
            foreach (var item in searchItems.EnumerateArray())
            {
                if (item.GetProperty("id").TryGetProperty("videoId", out var vidId))
                {
                    videoIds.Add(vidId.GetString()!);
                }
            }

            if (!videoIds.Any()) return new YouTubeResponse();

            var idsString = string.Join(",", videoIds);
            var statsUrl = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&id={idsString}&key={_apiKey}";
            var statsResponse = await _httpClient.GetAsync(statsUrl);

            var statsJson = await statsResponse.Content.ReadAsStringAsync();
            using var statsDoc = JsonDocument.Parse(statsJson);
            var items = statsDoc.RootElement.GetProperty("items");

            var videos = new List<VideoDto>();
            foreach (var item in items.EnumerateArray())
            {
                var snippet = item.GetProperty("snippet");
                var statistics = item.TryGetProperty("statistics", out var stats) ? stats : default;

                string viewsCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("viewCount", out var vCount) ? FormatViews(vCount.GetString()) : "0";
                if (viewsCount == "0") continue;

                string likesCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("likeCount", out var lCount)
                    ? FormatViews(lCount.GetString()!)
                    : "0";

                var thumbnails = snippet.GetProperty("thumbnails");
                bool hasHigh = thumbnails.TryGetProperty("high", out var high);
                bool hasMedium = thumbnails.TryGetProperty("medium", out var med);
                if (!hasHigh && !hasMedium) continue;

                videos.Add(new VideoDto
                {
                    Id = item.GetProperty("id").GetString()!,
                    Title = snippet.GetProperty("title").GetString() ?? "YouTube Short",
                    ChannelName = snippet.GetProperty("channelTitle").GetString() ?? "Unknown",
                    ThumbnailUrl = hasHigh ? high.GetProperty("url").GetString()! : med.GetProperty("url").GetString()!,
                    Views = viewsCount + " views",
                    Likes = likesCount,
                    PublishedAt = snippet.GetProperty("publishedAt").GetDateTime().ToString("MMM dd, yyyy"),
                    Description = snippet.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : ""
                });
            }

            return new YouTubeResponse { Videos = videos };
        }
    }
}