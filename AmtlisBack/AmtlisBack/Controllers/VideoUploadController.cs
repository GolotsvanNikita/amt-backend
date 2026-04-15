using AmtlisBack.Data;
using AmtlisBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AmtlisBack.Controllers
{
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
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile file, [FromForm] string title)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "videos");
            Directory.CreateDirectory(uploadsFolder);


            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var newVideo = new UploadedVideo
            {
                Title = title,
                VideoUrl = $"/uploads/videos/{uniqueFileName}",
                UserId = userId
            };

            _context.UploadedVideos.Add(newVideo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Upload successful", videoUrl = newVideo.VideoUrl });
        }
    }
}