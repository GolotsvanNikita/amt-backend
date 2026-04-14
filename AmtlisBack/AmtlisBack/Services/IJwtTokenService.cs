using AmtlisBack.Models;

namespace AmtlisBack.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}