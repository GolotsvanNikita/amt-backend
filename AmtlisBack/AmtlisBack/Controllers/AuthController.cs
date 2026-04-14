using AmtlisBack.Data;
using AmtlisBack.Models;
using AmtlisBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmtlisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(AppDbContext context, IPasswordService passwordService, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (request.Password != request.RepeatPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            string passwordHash = _passwordService.HashPassword(request.Password);

            var newUser = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            string token = _jwtTokenService.GenerateToken(newUser);

            return Ok(new
            {
                message = "Registration successful",
                user = new
                {
                    id = newUser.Id,
                    name = newUser.Name,
                    email = newUser.Email
                },
                token = token
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            bool isPasswordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            string token = _jwtTokenService.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                },
                token = token
            });
        }
    }
}