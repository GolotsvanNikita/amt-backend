using AmtlisBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmtlisBack.Controllers
{
    [Route("api/main-page")]
    [ApiController]
    public class MainPageController : ControllerBase
    {
        private readonly IYouTubeService _youTubeService;

        public MainPageController(IYouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }

        [HttpGet("videos")]
        public async Task<IActionResult> GetSections()
        {
            var top10 = await _youTubeService.GetVideosAsync(10, "", "10");
            var popular = await _youTubeService.GetVideosAsync(12, "", "");

            var sections = new[]
            {
                new { id = "top-week", title = "Top 10 on this week", videos = top10.Videos },
                new { id = "popular", title = "Popular", videos = popular.Videos }
            };

            return Ok(sections);
        }
    }
}