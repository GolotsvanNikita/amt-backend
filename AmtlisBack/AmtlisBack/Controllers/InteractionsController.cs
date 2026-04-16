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
    public class InteractionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InteractionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("video/{videoId}")]
        public async Task<IActionResult> GetVideoData(string videoId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.VideoId == videoId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new {
                    id = c.Id,
                    text = c.Text,
                    time = c.CreatedAt.ToString("MMM dd, yyyy"),
                    parentId = c.ParentId,
                    name = c.User != null ? c.User.Name : "User",
                    avatar = c.User != null ? c.User.AvatarUrl : "/ava.png"
                })
                .ToListAsync();

            var likesCount = await _context.VideoLikes.CountAsync(l => l.VideoId == videoId);

            return Ok(new { comments, likesCount });
        }

        [Authorize]
        [HttpPost("like/{videoId}")]
        public async Task<IActionResult> ToggleLike(string videoId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var existingLike = await _context.VideoLikes.FirstOrDefaultAsync(l => l.UserId == userId && l.VideoId == videoId);

            if (existingLike != null)
            {
                _context.VideoLikes.Remove(existingLike);
            }
            else
            {
                _context.VideoLikes.Add(new VideoLike { UserId = userId, VideoId = videoId });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpPost("subscribe")]
        public async Task<IActionResult> ToggleSubscribe([FromBody] SubscribeRequest req)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var existingSub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.ChannelName == req.ChannelName);

            if (existingSub != null)
            {
                _context.Subscriptions.Remove(existingSub);
            }
            else
            {
                _context.Subscriptions.Add(new ChannelSubscription { UserId = userId, ChannelName = req.ChannelName });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpPost("comment/{videoId}")]
        public async Task<IActionResult> AddComment(string videoId, [FromBody] CommentRequest req)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var comment = new Comment
            {
                UserId = userId,
                VideoId = videoId,
                Text = req.Text,
                ParentId = req.ParentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class SubscribeRequest { public string ChannelName { get; set; } = string.Empty; }
    public class CommentRequest { public string Text { get; set; } = string.Empty; public int? ParentId { get; set; } }
}