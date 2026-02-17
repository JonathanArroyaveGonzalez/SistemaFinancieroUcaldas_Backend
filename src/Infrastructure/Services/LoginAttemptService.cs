using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Domain.Entities;
using SAPFIAI.Domain.Enums;

namespace SAPFIAI.Infrastructure.Services;

public class LoginAttemptService : ILoginAttemptService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public LoginAttemptService(
        IApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task RecordAttemptAsync(
        string email, 
        string ipAddress, 
        bool wasSuccessful, 
        string? failureReason = null, 
        LoginFailureReason? failureReasonType = null, 
        string? userAgent = null)
    {
        var attempt = new LoginAttempt
        {
            Email = email,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AttemptDate = DateTime.UtcNow,
            WasSuccessful = wasSuccessful,
            FailureReason = failureReason,
            FailureReasonType = failureReasonType
        };

        _context.LoginAttempts.Add(attempt);
        await _context.SaveChangesAsync(default);
    }

    public async Task<int> GetRecentAttemptsByIpAsync(string ipAddress, int minutes = 15)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);

        return await _context.LoginAttempts
            .CountAsync(la => la.IpAddress == ipAddress && 
                             la.AttemptDate >= cutoffTime);
    }

    public async Task<int> GetRecentAttemptsByEmailAsync(string email, int minutes = 60)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);

        return await _context.LoginAttempts
            .CountAsync(la => la.Email == email && 
                             la.AttemptDate >= cutoffTime);
    }

    public async Task<int> GetFailedAttemptsByEmailAsync(string email, int minutes = 60)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);

        return await _context.LoginAttempts
            .CountAsync(la => la.Email == email && 
                             !la.WasSuccessful && 
                             la.AttemptDate >= cutoffTime);
    }

    public async Task<int> GetFailedAttemptsByIpAsync(string ipAddress, int minutes = 60)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);

        return await _context.LoginAttempts
            .CountAsync(la => la.IpAddress == ipAddress && 
                             !la.WasSuccessful && 
                             la.AttemptDate >= cutoffTime);
    }

    public async Task<bool> ShouldBlockIpAsync(string ipAddress)
    {
        var maxAttempts = _configuration.GetValue<int>("Security:BlackList:AutoBlockAfterAttempts", 10);
        var windowMinutes = _configuration.GetValue<int>("Security:RateLimiting:WindowMinutes", 60);

        var failedAttempts = await GetFailedAttemptsByIpAsync(ipAddress, windowMinutes);

        return failedAttempts >= maxAttempts;
    }

    public async Task<bool> ShouldLockAccountAsync(string email)
    {
        var maxAttempts = _configuration.GetValue<int>("Security:AccountLock:MaxFailedAttempts", 5);
        var windowMinutes = _configuration.GetValue<int>("Security:AccountLock:ResetFailedAttemptsAfterMinutes", 60);

        var failedAttempts = await GetFailedAttemptsByEmailAsync(email, windowMinutes);

        return failedAttempts >= maxAttempts;
    }

    public async Task<IEnumerable<LoginAttempt>> GetRecentAttemptsAsync(int count = 100)
    {
        return await _context.LoginAttempts
            .OrderByDescending(la => la.AttemptDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> CleanupOldAttemptsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldAttempts = await _context.LoginAttempts
            .Where(la => la.AttemptDate < cutoffDate)
            .ToListAsync();

        _context.LoginAttempts.RemoveRange(oldAttempts);

        return await _context.SaveChangesAsync(default);
    }
}
