using AmtlisBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmtlisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReelsController : ControllerBase
    {
        private readonly IYouTubeService _youTubeService;

        public ReelsController(IYouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {

            var categories = new[]
            {
                new { id = "all", title = "All", slug = "all" },
                new { id = "music", title = "Music", slug = "music" },
                new { id = "gaming", title = "Gaming", slug = "gaming" },
                new { id = "comedy", title = "Comedy", slug = "comedy" }
            };
            return Ok(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetReels()
        {
            var result = await _youTubeService.GetShortsAsync(15);

            var reels = result.Videos.Select(v => new
            {
                id = v.Id,
                title = v.Title,
                videoUrl = v.Id,
                imageUrl = v.ThumbnailUrl,
                posterUrl = v.ThumbnailUrl,
                avatarUrl = "/ava.png",
                categorySlug = "all",
                views = v.Views,
                time = v.PublishedAt,
                author = v.ChannelName,
                username = "@" + v.ChannelName.Replace(" ", "").ToLower(),
                description = v.Description,
                audioTitle = "Original Audio",
                likes = v.Likes,
                shares = new Random().Next(10, 1000),
                remix = new Random().Next(0, 100),
                isSubscribed = false,
                layoutType = GetRandomLayout(),
                comments = Array.Empty<object>()
            });

            return Ok(reels);
        }

        private string GetRandomLayout()
        {
            string[] layouts = { "small", "wide", "tall", "middleWide", "quote", "mediumTall", "smallTall", "bottomWide" };
            return layouts[new Random().Next(layouts.Length)];
        }
    }
}