using AmtlisBack.Data;
using AmtlisBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AmtlisBack.Controllers
{
    public class VideoUploadRequest
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public IFormFile? File { get; set; }
        public IFormFile? Thumbnail { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VideoUploadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public VideoUploadController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadVideo([FromForm] VideoUploadRequest req)
        {
            return await ProcessVideoUpload(req, isReel: false);
        }

        [HttpPost("upload-reel")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadReel([FromForm] VideoUploadRequest req)
        {
            return await ProcessVideoUpload(req, isReel: true);
        }

        private async Task<IActionResult> ProcessVideoUpload(VideoUploadRequest req, bool isReel)
        {
            try
            {
                if (req.File == null || req.File.Length == 0)
                    return BadRequest(new { message = "Video file is empty or missing." });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(new { message = "User not authorized." });

                string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                string folderName = isReel ? "reels" : "videos";
                string uploadsFolder = Path.Combine(webRoot, "uploads", folderName);
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(req.File.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await req.File.CopyToAsync(fileStream);
                }

                string thumbUrl = "/1v.png";
                if (req.Thumbnail != null && req.Thumbnail.Length > 0)
                {
                    string thumbFolder = Path.Combine(webRoot, "uploads", "thumbnails");
                    Directory.CreateDirectory(thumbFolder);
                    string thumbName = Guid.NewGuid().ToString() + Path.GetExtension(req.Thumbnail.FileName);
                    string thumbPath = Path.Combine(thumbFolder, thumbName);

                    using (var stream = new FileStream(thumbPath, FileMode.Create))
                    {
                        await req.Thumbnail.CopyToAsync(stream);
                    }
                    thumbUrl = $"/uploads/thumbnails/{thumbName}";
                }

                var newVideo = new UploadedVideo
                {
                    Title = string.IsNullOrWhiteSpace(req.Title) ? (isReel ? "New Reel" : "New Video") : req.Title,
                    Description = req.Description ?? "",
                    Category = req.Category ?? "",
                    VideoUrl = $"/uploads/{folderName}/{uniqueFileName}",
                    ThumbnailUrl = thumbUrl,
                    UserId = userId,
                    IsReel = isReel
                };

                _context.UploadedVideos.Add(newVideo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Upload successful", videoUrl = newVideo.VideoUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Server Error: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }
    }
}