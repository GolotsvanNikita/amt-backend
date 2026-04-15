using AmtlisBack.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AmtlisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("User not found");

            string displayUsername = string.IsNullOrEmpty(user.Username)
                ? user.Email.Split('@')[0]
                : user.Username;

            return Ok(new
            {
                name = user.Name,
                username = displayUsername,
                subscribers = user.SubscribersCount,
                avatar = user.AvatarUrl,
                bannerUrl = user.BannerUrl
            });
        }

        [HttpGet("videos")]
        public async Task<IActionResult> GetMyVideos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var videos = await _context.UploadedVideos
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.UploadedAt)
                .Select(v => new
                {
                    id = v.Id,
                    title = v.Title,
                    thumbnail = v.ThumbnailUrl,
                    views = v.Views.ToString() + " views",
                    time = v.UploadedAt.ToString("MMM dd, yyyy"),
                    videoUrl = v.VideoUrl
                })
                .ToListAsync();

            return Ok(videos);
        }
    }
}