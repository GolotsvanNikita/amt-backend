using AmtlisBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmtlisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IYouTubeService _youTubeService;

        public VideoController(IYouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending()
        {
            var result = await _youTubeService.GetVideosAsync(12);

            return Ok(result.Videos);
        }

        [HttpGet("top10")]
        public async Task<IActionResult> GetTop10()
        {
            var result = await _youTubeService.GetVideosAsync(10);
            return Ok(result.Videos);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllVideos([FromQuery] string pageToken = "")
        {
            var result = await _youTubeService.GetVideosAsync(12, pageToken);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string pageToken = "")
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query is required");

            var result = await _youTubeService.SearchVideosAsync(q, 15, pageToken);
            return Ok(result);
        }
    }
}