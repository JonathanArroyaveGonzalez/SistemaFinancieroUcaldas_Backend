namespace SAPFIAI.Application.Common.Interfaces;

public interface IAccountLockService
{
    Task<bool> LockAccountAsync(string userId, int lockoutMinutes = 15);
    
    Task<bool> UnlockAccountAsync(string userId);
    
    Task<bool> IsAccountLockedAsync(string userId);
    
    Task<int> IncrementFailedAttemptsAsync(string userId);
    
    Task ResetFailedAttemptsAsync(string userId);
    
    Task<(bool isLocked, DateTime? lockoutEnd, int failedAttempts)> GetAccountLockStatusAsync(string userId);
}
