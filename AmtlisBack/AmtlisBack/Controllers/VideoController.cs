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
            // Просим 10 видео (можно добавить категорию, например "10" для музыки)
            var result = await _youTubeService.GetVideosAsync(10);
            return Ok(result.Videos); // Отдаем только массив, токен тут не нужен
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllVideos([FromQuery] string pageToken = "")
        {
            // Просим 12 видео и передаем токен страницы (если фронт его прислал)
            var result = await _youTubeService.GetVideosAsync(12, pageToken);
            return Ok(result); // Отдаем и видео, и NextPageToken для бесконечной прокрутки
        }
    }
}