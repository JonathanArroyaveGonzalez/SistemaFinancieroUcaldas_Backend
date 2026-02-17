using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress);
    
    Task<(bool isValid, string? userId, string? email)> ValidateRefreshTokenAsync(string token, string ipAddress);
    
    Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress, string reason);
    
    Task<int> RevokeAllUserTokensAsync(string userId, string ipAddress, string reason);
    
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId);
    
    Task<int> CleanupExpiredTokensAsync();
}
