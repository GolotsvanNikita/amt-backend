using AmtlisBack.Data;
using AmtlisBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AmtlisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordView([FromBody] WatchHistory request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var existing = await _context.WatchHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.VideoId == request.VideoId);

            if (existing != null)
            {
                existing.WatchedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(request.AvatarUrl) && request.AvatarUrl != "/ava.png")
                {
                    existing.AvatarUrl = request.AvatarUrl;
                }
            }
            else
            {
                request.UserId = userId;
                request.WatchedAt = DateTime.UtcNow;
                _context.WatchHistories.Add(request);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out int userId)) return BadRequest("Invalid ID format");

            var rawHistory = await _context.WatchHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.WatchedAt)
                .Take(30)
                .ToListAsync();

            var distinctHistory = rawHistory
                .GroupBy(h => h.VideoId)
                .Select(g => g.First())
                .Take(4)
                .Select(h => new
                {
                    videoId = h.VideoId,
                    title = h.Title,
                    thumbnailUrl = h.ThumbnailUrl,
                    channelName = h.ChannelName,
                    channelId = h.ChannelId ?? "",
                    channelAvatarUrl = h.AvatarUrl ?? "/ava.png",
                    viewsCount = h.ViewsCount ?? "0",
                    likesCount = h.LikesCount ?? "0",
                    durationSeconds = h.DurationSeconds,
                    lastPositionSeconds = h.LastPositionSeconds
                })
                .ToList();

            return Ok(distinctHistory);
        }

        [HttpPost("progress")]
        public async Task<IActionResult> UpdateProgress([FromBody] ProgressRequestDto req)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var existing = await _context.WatchHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.VideoId == req.VideoId);

            int progressPercent = 0;
            if (req.DurationSeconds > 0)
            {
                progressPercent = (int)Math.Round((double)req.LastPositionSeconds / req.DurationSeconds * 100);
            }
            bool isFinished = progressPercent >= 95;

            if (existing != null)
            {
                existing.Title = req.Title;
                existing.ThumbnailUrl = req.ThumbnailUrl;
                existing.ChannelName = req.ChannelName;
                existing.ChannelId = req.ChannelId;

                if (!string.IsNullOrEmpty(req.ChannelAvatarUrl) && req.ChannelAvatarUrl != "/ava.png")
                {
                    existing.AvatarUrl = req.ChannelAvatarUrl;
                }

                existing.ViewsCount = req.ViewsCount;
                existing.LikesCount = req.LikesCount;

                existing.DurationSeconds = req.DurationSeconds;
                existing.LastPositionSeconds = req.LastPositionSeconds;
                existing.ProgressPercent = progressPercent;
                existing.IsFinished = isFinished;
                existing.WatchedAt = DateTime.UtcNow;
            }
            else
            {
                var newHistory = new WatchHistory
                {
                    UserId = userId,
                    VideoId = req.VideoId,
                    Title = req.Title,
                    ThumbnailUrl = req.ThumbnailUrl,
                    ChannelName = req.ChannelName,
                    ChannelId = req.ChannelId,
                    AvatarUrl = req.ChannelAvatarUrl ?? "/ava.png",
                    ViewsCount = req.ViewsCount,
                    LikesCount = req.LikesCount,

                    DurationSeconds = req.DurationSeconds,
                    LastPositionSeconds = req.LastPositionSeconds,
                    ProgressPercent = progressPercent,
                    IsFinished = isFinished,
                    WatchedAt = DateTime.UtcNow
                };
                _context.WatchHistories.Add(newHistory);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}