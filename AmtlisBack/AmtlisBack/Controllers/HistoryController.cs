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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var existing = await _context.WatchHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.VideoId == request.VideoId);

            if (existing != null)
            {
                existing.WatchedAt = DateTime.UtcNow;
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var rawHistory = await _context.WatchHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.WatchedAt)
                .Take(30)
                .ToListAsync();

            var distinctHistory = rawHistory
                .GroupBy(h => h.VideoId)
                .Select(g => g.First())
                .Take(4)
                .ToList();

            return Ok(distinctHistory);
        }
    }
}