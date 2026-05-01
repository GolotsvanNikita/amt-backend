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

        private string FormatViews(string views)
        {
            if (long.TryParse(views, out long count))
            {
                if (count >= 1000000) return (count / 1000000D).ToString("0.#") + "M";
                if (count >= 1000) return (count / 1000D).ToString("0.#") + "K";
            }
            return views;
        }

        private async Task AttachAvatarsAsync(List<VideoDto> videos)
        {
            if (!videos.Any()) return;

            var uniqueChannelIds = videos.Select(v => v.ChannelId).Where(id => !string.IsNullOrEmpty(id)).Distinct().Take(50).ToList();
            if (!uniqueChannelIds.Any()) return;

            var channelsString = string.Join(",", uniqueChannelIds);
            var channelsUrl = $"https://www.googleapis.com/youtube/v3/channels?part=snippet&id={channelsString}&key={_apiKey}";
            var channelsResponse = await _httpClient.GetAsync(channelsUrl);

            if (channelsResponse.IsSuccessStatusCode)
            {
                var channelsJson = await channelsResponse.Content.ReadAsStringAsync();
                using var channelsDoc = JsonDocument.Parse(channelsJson);

                if (channelsDoc.RootElement.TryGetProperty("items", out var channelItems))
                {
                    var avatarDict = new Dictionary<string, string>();

                    foreach (var cItem in channelItems.EnumerateArray())
                    {
                        var cId = cItem.GetProperty("id").GetString()!;
                        var thumbnails = cItem.GetProperty("snippet").GetProperty("thumbnails");

                        string avatar = thumbnails.TryGetProperty("default", out var def)
                            ? def.GetProperty("url").GetString()!
                            : "/ava.png";

                        avatarDict[cId] = avatar;
                    }

                    foreach (var video in videos)
                    {
                        if (avatarDict.TryGetValue(video.ChannelId, out var avatar))
                        {
                            video.ChannelAvatarUrl = avatar;
                        }
                    }
                }
            }
        }

        public async Task<YouTubeResponse> GetVideosAsync(int maxResults, string pageToken = "", string categoryId = "")
        {
            var url = $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics&chart=mostPopular&maxResults={maxResults}&regionCode=US&key={_apiKey}";

            if (!string.IsNullOrEmpty(pageToken)) url += $"&pageToken={pageToken}";
            if (!string.IsNullOrEmpty(categoryId)) url += $"&videoCategoryId={categoryId}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new YouTubeResponse();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonString);
            var items = document.RootElement.GetProperty("items");

            string nextToken = document.RootElement.TryGetProperty("nextPageToken", out var tokenProp) ? tokenProp.GetString() ?? "" : "";
            var videos = new List<VideoDto>();

            foreach (var item in items.EnumerateArray())
            {
                var snippet = item.GetProperty("snippet");
                var statistics = item.TryGetProperty("statistics", out var stats) ? stats : default;

                string viewsCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("viewCount", out var vCount) ? FormatViews(vCount.GetString()!) : "0";
                string likesCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("likeCount", out var lCount) ? FormatViews(lCount.GetString()!) : "0";

                if (viewsCount == "0") continue;

                var thumbnails = snippet.GetProperty("thumbnails");
                bool hasHigh = thumbnails.TryGetProperty("high", out var high);
                bool hasMedium = thumbnails.TryGetProperty("medium", out var med);

                if (!hasHigh && !hasMedium) continue;

                string? thumbUrl = hasHigh ? high.GetProperty("url").GetString() : med.GetProperty("url").GetString();

                videos.Add(new VideoDto
                {
                    Id = item.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                    ChannelId = snippet.GetProperty("channelId").GetString()!,
                    Title = snippet.GetProperty("title").GetString() ?? "Unknown Title",
                    ChannelName = snippet.GetProperty("channelTitle").GetString() ?? "Unknown Channel",
                    ThumbnailUrl = thumbUrl ?? "",
                    Views = viewsCount + " views",
                    Likes = likesCount,
                    PublishedAt = snippet.GetProperty("publishedAt").GetDateTime().ToString("MMM dd, yyyy"),
                    Description = snippet.TryGetProperty("description", out JsonElement desc) ? desc.GetString() ?? "" : ""
                });
            }

            await AttachAvatarsAsync(videos);
            return new YouTubeResponse { Videos = videos, NextPageToken = nextToken };
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

                string viewsCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("viewCount", out var vCount) ? FormatViews(vCount.GetString()!) : "0";
                if (viewsCount == "0") continue;

                string likesCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("likeCount", out var lCount) ? FormatViews(lCount.GetString()!) : "0";
                string commentsCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("commentCount", out var cCount) ? FormatViews(cCount.GetString()!) : "0";

                var thumbnails = snippet.GetProperty("thumbnails");
                bool hasHigh = thumbnails.TryGetProperty("high", out var high);
                bool hasMedium = thumbnails.TryGetProperty("medium", out var med);
                if (!hasHigh && !hasMedium) continue;

                videos.Add(new VideoDto
                {
                    Id = item.GetProperty("id").GetString()!,
                    ChannelId = snippet.GetProperty("channelId").GetString()!,
                    Title = snippet.GetProperty("title").GetString() ?? "YouTube Short",
                    ChannelName = snippet.GetProperty("channelTitle").GetString() ?? "Unknown",
                    ThumbnailUrl = hasHigh ? high.GetProperty("url").GetString()! : med.GetProperty("url").GetString()!,
                    Views = viewsCount + " views",
                    Likes = likesCount,
                    CommentsCount = commentsCount,
                    PublishedAt = snippet.GetProperty("publishedAt").GetDateTime().ToString("MMM dd, yyyy"),
                    Description = snippet.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : ""
                });
            }

            await AttachAvatarsAsync(videos);
            return new YouTubeResponse { Videos = videos };
        }

        public async Task<List<CommentDto>> GetVideoCommentsAsync(string videoId, int maxResults = 20)
        {
            var url = $"https://www.googleapis.com/youtube/v3/commentThreads?part=snippet,replies&videoId={videoId}&maxResults={maxResults}&key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            var comments = new List<CommentDto>();

            if (!response.IsSuccessStatusCode) return comments;

            var jsonString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonString);
            if (!document.RootElement.TryGetProperty("items", out var items)) return comments;

            foreach (var item in items.EnumerateArray())
            {
                var topLevel = item.GetProperty("snippet").GetProperty("topLevelComment").GetProperty("snippet");

                var comment = new CommentDto
                {
                    Id = item.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                    Name = topLevel.TryGetProperty("authorDisplayName", out var author) ? author.GetString() ?? "Unknown" : "Unknown",
                    Avatar = topLevel.TryGetProperty("authorProfileImageUrl", out var avatar) ? avatar.GetString() ?? "/ava.png" : "/ava.png",
                    Text = topLevel.TryGetProperty("textOriginal", out var text) ? text.GetString() ?? "" : "",
                    Time = topLevel.TryGetProperty("publishedAt", out var pub) ? pub.GetDateTime().ToString("MMM dd, yyyy") : "Unknown",
                    Replies = []
                };

                if (item.TryGetProperty("replies", out var repliesProp) && repliesProp.TryGetProperty("comments", out var commentsArray))
                {
                    foreach (var replyItem in commentsArray.EnumerateArray())
                    {
                        var replySnippet = replyItem.GetProperty("snippet");
                        comment.Replies.Add(new CommentDto
                        {
                            Id = replyItem.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                            Name = replySnippet.TryGetProperty("authorDisplayName", out var rAuthor) ? rAuthor.GetString() ?? "Unknown" : "Unknown",
                            Avatar = replySnippet.TryGetProperty("authorProfileImageUrl", out var rAvatar) ? rAvatar.GetString() ?? "/ava.png" : "/ava.png",
                            Text = replySnippet.TryGetProperty("textOriginal", out var rText) ? rText.GetString() ?? "" : "",
                            Time = replySnippet.TryGetProperty("publishedAt", out var rPub) ? rPub.GetDateTime().ToString("MMM dd, yyyy") : "Unknown",
                            ParentId = comment.Id
                        });
                    }
                }
                comments.Add(comment);
            }
            return comments;
        }

        public async Task<ChannelDto?> GetChannelAsync(string channelId)
        {
            var url = $"https://www.googleapis.com/youtube/v3/channels?part=snippet,statistics,contentDetails,brandingSettings&id={channelId}&key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            if (!doc.RootElement.TryGetProperty("items", out var items) || items.GetArrayLength() == 0) return null;

            var item = items[0];
            var snippet = item.GetProperty("snippet");
            var stats = item.GetProperty("statistics");

            var uploadsPlaylistId = item.GetProperty("contentDetails").GetProperty("relatedPlaylists").GetProperty("uploads").GetString()!;

            string bannerUrl = string.Empty;

            if (item.TryGetProperty("brandingSettings", out var branding) &&
                branding.TryGetProperty("image", out var image))
            {
                if (image.TryGetProperty("bannerExternalUrl", out var bUrlExt))
                {
                    bannerUrl = bUrlExt.GetString() ?? string.Empty;
                }
            }

            return new ChannelDto
            {
                Id = item.GetProperty("id").GetString()!,
                Title = snippet.GetProperty("title").GetString() ?? "Unknown",
                Description = snippet.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                AvatarUrl = snippet.GetProperty("thumbnails").GetProperty("high").GetProperty("url").GetString() ?? "/ava.png",
                SubscriberCount = stats.TryGetProperty("subscriberCount", out var subCount) ? FormatViews(subCount.GetString()!) : "0",
                CustomUrl = snippet.TryGetProperty("customUrl", out var cUrl) ? cUrl.GetString() ?? "" : "",
                UploadsPlaylistId = uploadsPlaylistId,
                BannerUrl = bannerUrl
            };
        }

        public async Task<YouTubeResponse> GetVideosFromPlaylistAsync(string playlistId, int maxResults = 10)
        {
            var plUrl = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={playlistId}&maxResults={maxResults}&key={_apiKey}";
            var plResponse = await _httpClient.GetAsync(plUrl);
            if (!plResponse.IsSuccessStatusCode) return new YouTubeResponse();

            var plJson = await plResponse.Content.ReadAsStringAsync();
            using var plDoc = JsonDocument.Parse(plJson);
            var plItems = plDoc.RootElement.GetProperty("items");

            var videoIds = new List<string>();
            foreach (var item in plItems.EnumerateArray())
            {
                videoIds.Add(item.GetProperty("snippet").GetProperty("resourceId").GetProperty("videoId").GetString()!);
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
                var statistics = item.TryGetProperty("statistics", out var stat) ? stat : default;

                string viewsCount = statistics.ValueKind != JsonValueKind.Undefined && statistics.TryGetProperty("viewCount", out var vCount) ? FormatViews(vCount.GetString()!) : "0";

                videos.Add(new VideoDto
                {
                    Id = item.GetProperty("id").GetString()!,
                    ChannelId = snippet.GetProperty("channelId").GetString()!,
                    Title = snippet.GetProperty("title").GetString() ?? "Unknown",
                    ChannelName = snippet.GetProperty("channelTitle").GetString() ?? "Unknown",
                    ThumbnailUrl = snippet.GetProperty("thumbnails").GetProperty("high").GetProperty("url").GetString()!,
                    Views = viewsCount + " views",
                    PublishedAt = snippet.GetProperty("publishedAt").GetDateTime().ToString("MMM dd, yyyy")
                });
            }

            await AttachAvatarsAsync(videos);
            return new YouTubeResponse { Videos = videos };
        }

        public async Task<List<PlaylistDto>> GetChannelPlaylistsAsync(string channelId, int maxResults = 5)
        {
            var url = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet,contentDetails&channelId={channelId}&maxResults={maxResults}&key={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            var playlists = new List<PlaylistDto>();
            if (!response.IsSuccessStatusCode) return playlists;

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            if (!doc.RootElement.TryGetProperty("items", out var items)) return playlists;

            foreach (var item in items.EnumerateArray())
            {
                var snippet = item.GetProperty("snippet");
                var contentDetails = item.GetProperty("contentDetails");

                var thumbnails = snippet.GetProperty("thumbnails");
                string thumbUrl = thumbnails.TryGetProperty("medium", out var med)
                    ? med.GetProperty("url").GetString()!
                    : thumbnails.GetProperty("default").GetProperty("url").GetString()!;

                playlists.Add(new PlaylistDto
                {
                    Id = item.GetProperty("id").GetString()!,
                    Title = snippet.GetProperty("title").GetString() ?? "Unknown Playlist",
                    ThumbnailUrl = thumbUrl,
                    VideoCount = contentDetails.TryGetProperty("itemCount", out var count) ? count.GetInt32() : 0
                });
            }

            return playlists;
        }

        public async Task<YouTubeResponse> SearchVideosAsync(string query, int maxResults = 15, string pageToken = "")
        {
            var searchUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={Uri.EscapeDataString(query)}&type=video&maxResults={maxResults}&key={_apiKey}";
            if (!string.IsNullOrEmpty(pageToken)) searchUrl += $"&pageToken={pageToken}";

            var searchResponse = await _httpClient.GetAsync(searchUrl);
            if (!searchResponse.IsSuccessStatusCode) return new YouTubeResponse();

            var searchJson = await searchResponse.Content.ReadAsStringAsync();
            using var searchDoc = JsonDocument.Parse(searchJson);
            var items = searchDoc.RootElement.GetProperty("items");

            string nextToken = searchDoc.RootElement.TryGetProperty("nextPageToken", out var token) ? token.GetString() ?? "" : "";

            var videos = new List<VideoDto>();
            var videoIds = new List<string>();

            foreach (var item in items.EnumerateArray())
            {
                var snippet = item.GetProperty("snippet");
                var idObj = item.GetProperty("id");
                string videoId = idObj.TryGetProperty("videoId", out var vId) ? vId.GetString()! : "";

                if (string.IsNullOrEmpty(videoId)) continue;

                videoIds.Add(videoId);

                var thumbnails = snippet.GetProperty("thumbnails");
                string thumbUrl = thumbnails.TryGetProperty("high", out var high)
                    ? high.GetProperty("url").GetString()!
                    : thumbnails.GetProperty("default").GetProperty("url").GetString()!;

                videos.Add(new VideoDto
                {
                    Id = videoId,
                    ChannelId = snippet.GetProperty("channelId").GetString()!,
                    Title = snippet.GetProperty("title").GetString() ?? "Unknown",
                    ChannelName = snippet.GetProperty("channelTitle").GetString() ?? "Unknown",
                    ThumbnailUrl = thumbUrl,

                    PublishedAt = snippet.TryGetProperty("publishedAt", out var pub) ? pub.GetDateTime().ToString("MMM dd, yyyy") : "",
                    Description = snippet.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : ""
                });
            }

            if (videoIds.Any())
            {
                var statsUrl = $"https://www.googleapis.com/youtube/v3/videos?part=statistics&id={string.Join(",", videoIds)}&key={_apiKey}";
                var statsResponse = await _httpClient.GetAsync(statsUrl);

                if (statsResponse.IsSuccessStatusCode)
                {
                    var statsJson = await statsResponse.Content.ReadAsStringAsync();
                    using var statsDoc = JsonDocument.Parse(statsJson);

                    if (statsDoc.RootElement.TryGetProperty("items", out var statsItems))
                    {
                        var viewsDict = new Dictionary<string, string>();

                        foreach (var sItem in statsItems.EnumerateArray())
                        {
                            string vId = sItem.GetProperty("id").GetString()!;
                            string views = sItem.GetProperty("statistics").TryGetProperty("viewCount", out var vc)
                                ? FormatViews(vc.GetString()!)
                                : "0";

                            viewsDict[vId] = views;
                        }

                        foreach (var video in videos)
                        {
                            if (viewsDict.TryGetValue(video.Id, out var views))
                            {
                                video.Views = views;
                            }
                        }
                    }
                }
            }

            await AttachAvatarsAsync(videos);
            return new YouTubeResponse { Videos = videos, NextPageToken = nextToken };
        }
    }
}