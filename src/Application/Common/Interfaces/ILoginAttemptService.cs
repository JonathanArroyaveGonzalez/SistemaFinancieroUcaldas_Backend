using SAPFIAI.Domain.Entities;
using SAPFIAI.Domain.Enums;

namespace SAPFIAI.Application.Common.Interfaces;

public interface ILoginAttemptService
{
    Task RecordAttemptAsync(string email, string ipAddress, bool wasSuccessful, string? failureReason = null, LoginFailureReason? failureReasonType = null, string? userAgent = null);
    
    Task<int> GetRecentAttemptsByIpAsync(string ipAddress, int minutes = 15);
    
    Task<int> GetRecentAttemptsByEmailAsync(string email, int minutes = 60);
    
    Task<int> GetFailedAttemptsByEmailAsync(string email, int minutes = 60);
    
    Task<int> GetFailedAttemptsByIpAsync(string ipAddress, int minutes = 60);
    
    Task<bool> ShouldBlockIpAsync(string ipAddress);
    
    Task<bool> ShouldLockAccountAsync(string email);
    
    Task<IEnumerable<LoginAttempt>> GetRecentAttemptsAsync(int count = 100);
    
    Task<int> CleanupOldAttemptsAsync(int daysToKeep = 30);
}
