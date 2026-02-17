using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthenticationOperations _authOperations;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(
        IApplicationDbContext context,
        IAuthenticationOperations authOperations,
        IConfiguration configuration)
    {
        _context = context;
        _authOperations = authOperations;
        _configuration = configuration;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress)
    {
        var expirationDays = _configuration.GetValue<int>("Security:RefreshToken:ExpirationDays", 7);
        var maxActiveTokens = _configuration.GetValue<int>("Security:RefreshToken:MaxActiveTokensPerUser", 5);

        // Revocar tokens antiguos si se excede el límite
        var activeTokens = await GetActiveTokensByUserAsync(userId);
        if (activeTokens.Count() >= maxActiveTokens)
        {
            var oldestToken = activeTokens.OrderBy(t => t.CreatedDate).First();
            await RevokeRefreshTokenAsync(oldestToken.Token, ipAddress, "Exceeded maximum active tokens");
        }

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(expirationDays),
            CreatedDate = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(default);

        return refreshToken;
    }

    public async Task<(bool isValid, string? userId, string? email)> ValidateRefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return (false, null, null);

        if (refreshToken.IsRevoked)
            return (false, null, null);

        if (refreshToken.IsExpired)
            return (false, null, null);

        // Obtener email del usuario
        var user = await _authOperations.GetUserByIdAsync(refreshToken.UserId);
        if (user == null)
            return (false, null, null);

        return (true, refreshToken.UserId, user.Email);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress, string reason)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || refreshToken.IsRevoked)
            return false;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedDate = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = reason;

        await _context.SaveChangesAsync(default);

        return true;
    }

    public async Task<int> RevokeAllUserTokensAsync(string userId, string ipAddress, string reason)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedDate = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
        }

        await _context.SaveChangesAsync(default);

        return activeTokens.Count;
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedDate)
            .ToListAsync();
    }

    public async Task<int> CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate < DateTime.UtcNow)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);

        return await _context.SaveChangesAsync(default);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
