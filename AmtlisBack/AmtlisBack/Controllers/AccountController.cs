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
        private readonly IWebHostEnvironment _env;

        public AccountController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                username = user.Username,
                about = user.About,
                color = user.Color,
                avatarUrl = user.AvatarUrl,
                bannerUrl = user.BannerUrl,
                subscribers = user.SubscribersCount
            });
        }

        [Authorize]
        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound("User not found");

            if (!string.IsNullOrEmpty(dto.Name)) user.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Username)) user.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.Color)) user.Color = dto.Color;
            if (!string.IsNullOrEmpty(dto.AvatarUrl)) user.AvatarUrl = dto.AvatarUrl;
            if (!string.IsNullOrEmpty(dto.BannerUrl)) user.BannerUrl = dto.BannerUrl;
            if (dto.About != null) user.About = dto.About;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
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

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            return await UploadImageHelper(file, "avatars");
        }

        [HttpPost("upload-banner")]
        public async Task<IActionResult> UploadBanner([FromForm] IFormFile file)
        {
            return await UploadImageHelper(file, "banners");
        }

        private async Task<IActionResult> UploadImageHelper(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is empty" });

            string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", subFolder);
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);


            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fileUrl = $"{baseUrl}/uploads/{subFolder}/{uniqueFileName}";

            return Ok(new { url = fileUrl });
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var subscriptions = await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .Select(s => new {
                    channelName = s.ChannelName,
                    avatarUrl = s.AvatarUrl,
                    channelId = s.ChannelId
                })
                .ToListAsync();

            return Ok(subscriptions);
        }

        [HttpGet("videos/popular")]
        public async Task<IActionResult> GetPopularVideos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var videos = await _context.UploadedVideos
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.Views)
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

        [HttpGet("playlists")]
        public IActionResult GetPlaylists()
        {
            return Ok(Array.Empty<object>());
        }

        [HttpGet("achievement")]
        public IActionResult GetAchievements()
        {
            return Ok(Array.Empty<object>());
        }
    }
}