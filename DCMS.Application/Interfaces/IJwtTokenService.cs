using DCMS.Domain.Entities;

namespace DCMS.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    int GetUserIdFromExpiredToken(string token);
}
