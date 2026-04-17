using AmtlisBack.Data;
using AmtlisBack.Models;
using AmtlisBack.Services;
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
        private readonly IYouTubeService _youTubeService;

        public InteractionsController(AppDbContext context, IYouTubeService youTubeService)
        {
            _context = context;
            _youTubeService = youTubeService;
        }

        [HttpGet("video/{videoId}")]
        public async Task<IActionResult> GetVideoData(string videoId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var commentsDb = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.VideoId == videoId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var localComments = commentsDb.Select(c => new CommentDto
            {
                Id = "local_" + c.Id.ToString(),
                Text = c.Text,
                Time = c.CreatedAt.ToString("MMM dd, yyyy"),
                ParentId = c.ParentId != null ? "local_" + c.ParentId.ToString() : null,
                Name = c.User != null ? c.User.Name : "User",
                Avatar = c.User != null ? c.User.AvatarUrl : "/ava.png",
                Replies = new List<CommentDto>()
            }).ToList();

            var topLevelLocal = localComments.Where(c => c.ParentId == null).ToList();
            foreach (var top in topLevelLocal)
            {
                top.Replies = localComments.Where(c => c.ParentId == top.Id).ToList();
            }

            var ytComments = await _youTubeService.GetVideoCommentsAsync(videoId, 50);

            var allComments = topLevelLocal.Concat(ytComments).ToList();

            var pagedComments = allComments.Skip((page - 1) * limit).Take(limit).ToList();
            bool hasMore = (page * limit) < allComments.Count;

            var likesCount = await _context.VideoLikes.CountAsync(l => l.VideoId == videoId);

            return Ok(new
            {
                comments = pagedComments,
                likesCount = likesCount,
                hasMoreComments = hasMore,
                page = page
            });
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
                _context.Subscriptions.Add(new ChannelSubscription
                { 
                    UserId = userId,
                    ChannelName = req.ChannelName,
                    ChannelId = req.ChannelId,
                    AvatarUrl = req.AvatarUrl
                });
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

    public class SubscribeRequest
    {
        public string ChannelName { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "/ava.png";
    }
    public class CommentRequest { public string Text { get; set; } = string.Empty; public int? ParentId { get; set; } }
}