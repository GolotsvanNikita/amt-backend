using AmtlisBack.Data;
using AmtlisBack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AmtlisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly IYouTubeService _youTubeService;
        private readonly AppDbContext _context;

        public ChannelController(IYouTubeService youTubeService, AppDbContext context)
        {
            _youTubeService = youTubeService;
            _context = context;
        }

        [HttpGet("{channelId}")]
        public async Task<IActionResult> GetChannelPage(string channelId)
        {
            var channel = await _youTubeService.GetChannelAsync(channelId);
            if (channel == null) return NotFound("Channel not found");

            var videos = await _youTubeService.GetVideosFromPlaylistAsync(channel.UploadsPlaylistId, 12);

            var playlists = await _youTubeService.GetChannelPlaylistsAsync(channelId, 5);

            bool isSubscribed = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                isSubscribed = await _context.Subscriptions.AnyAsync(s => s.UserId == userId && s.ChannelName == channel.Title);
            }

            return Ok(new
            {
                channel = channel,
                videos = videos.Videos,
                featuredVideo = videos.Videos.FirstOrDefault(),
                playlists = playlists,
                isSubscribed = isSubscribed
            });
        }
    }
}